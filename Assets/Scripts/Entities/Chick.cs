using System.Collections.Generic;
using UnityEngine;

public class Chick : PlaceableEntity
{
    private static readonly List<PlaceableEntity> contagionBuffer = new List<PlaceableEntity>();

    [SerializeField] private ChickData data;
    [SerializeField] private MutationData mutationData;

    private float contagionTimer;
    private bool contagionActive;

    private readonly MirrorExitMotion mirrorExitMotion = new MirrorExitMotion();
    private bool mirrorExitMasked;
    private Bounds? mirrorExitMaskBounds;

    public override EconomyEntityData EconomyData => data;
    public override bool CanBeCarried => base.CanBeCarried && !IsMirrorExiting;

    public float CurrentDuckGrowthChance { get; private set; }

    public bool IsGuided { get; private set; }

    protected override void OnEnable()
    {
        base.OnEnable();

        mirrorExitMotion.Cancel();
        spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
        spriteRenderer.sortingLayerName = "Objects";
        ResetLifeState();
    }

    private void ResetLifeState()
    {
        IsGuided = false;
        ResetWalk(data);
        ResetAmbient(AmbientSoundKind.Chick);

        if (data.IsMutant)
        {
            contagionTimer = mutationData != null ? NextContagionInterval(mutationData) : 0f;
        }
    }

    private static float NextContagionInterval(MutationData mutation)
    {
        return mutation.ContagionInterval +
            Random.Range(mutation.ContagionTimerJitterRange.x, mutation.ContagionTimerJitterRange.y);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        mirrorExitMotion.Cancel();
        IsGuided = false;
    }

    public override void RefreshStats()
    {
        base.RefreshStats();

        UpgradeManager manager = UpgradeManager.Instance;
        contagionActive = manager != null && manager.State.Get<ContagionState>().Enabled;

        CurrentDuckGrowthChance = ApplyModifier(ShopStatType.DuckGrowthChance, data.DuckGrowthChance);
    }

    protected override GameObject PickNextStagePrefab(EconomyEntityData economyData)
    {
        if (data.DuckStagePrefab != null && CurrentDuckGrowthChance > 0f &&
            Random.value <= CurrentDuckGrowthChance)
        {
            return data.DuckStagePrefab;
        }

        return base.PickNextStagePrefab(economyData);
    }

    protected override void TickBehaviour(float dt)
    {
        if (mirrorExitMotion.IsActive)
        {
            TickMirrorExit(dt);
            return;
        }

        TickWalk(data, dt);

        if (IsGuided && !IsWalkMoving)
        {
            IsGuided = false;
        }

        TickAmbient(AmbientSoundKind.Chick, dt);
    }

    protected override void SlowTickBehaviour(float dt)
    {
        if (mirrorExitMotion.IsActive)
        {
            return;
        }

        if (data.IsMutant)
        {
            TickContagion(dt);
        }

        TickGrowth(dt);
    }

    public override void SetCarried(bool carried)
    {
        base.SetCarried(carried);

        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        if (carried)
        {
            flipbook.Play(data.IdleAnimation);
        }
        else
        {
            ResetLifeState();
        }
    }

    protected override bool CanGrowNow()
    {
        SpawnManager spawn = SpawnManager.Instance;
        return spawn == null || ChickGrowthRule.IsNotLastOfKind(data.IsMutant, spawn.ChickCount, spawn.MutantChickCount);
    }

    public bool IsMirrorExiting => mirrorExitMotion.IsActive;

    protected override void OnMergeBegin()
    {
        IsGuided = false;
    }

    protected override void OnMergeCancel()
    {
        ResetLifeState();
    }

    public void GuideTo(Vector3 destination)
    {
        if (IsCarried || IsMirrorExiting || !gameObject.activeInHierarchy)
        {
            return;
        }

        IsGuided = true;
        WalkTo(destination, data);
    }

    public void ForceMirrorExit(Vector2 dir, MirrorData mirrorData, bool masked, Bounds? maskBounds = null)
    {
        IsGuided = false;
        spriteRenderer.maskInteraction =
            masked ? SpriteMaskInteraction.VisibleOutsideMask : SpriteMaskInteraction.None;
        if (masked)
        {
            spriteRenderer.sortingLayerName = "Reflection";
        }

        flipbook.Play(data.WalkAnimation);

        float exitMultiplier = data.IsMutant
            ? mirrorData.MutantExitSpeedMultiplier
            : mirrorData.ExitSpeedMultiplier;

        mirrorExitMasked = masked;
        mirrorExitMaskBounds = maskBounds;
        mirrorExitMotion.Begin(
            transform.position,
            dir,
            data.MoveSpeed * exitMultiplier,
            mirrorData.ForcedExitDistance,
            mirrorData.RandomExitAngle,
            mirrorData.RandomExitDuration);

        spriteRenderer.flipX = mirrorExitMotion.Direction.x > 0f;
    }

    private void TickMirrorExit(float dt)
    {
        Vector3 position = transform.position;
        bool directionChanged = mirrorExitMotion.Tick(dt, ref position);
        transform.position = position;
        SyncSpatialCell();

        if (directionChanged)
        {
            spriteRenderer.flipX = mirrorExitMotion.Direction.x > 0f;
        }

        if (mirrorExitMasked && mirrorExitMaskBounds.HasValue && !mirrorExitMaskBounds.Value.Intersects(spriteRenderer.bounds))
        {
            RestoreObjectsLayer();
        }

        if (!mirrorExitMotion.IsActive)
        {
            FinishMirrorExit();
        }
    }

    private void RestoreObjectsLayer()
    {
        if (!mirrorExitMasked)
        {
            return;
        }

        spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
        spriteRenderer.sortingLayerName = "Objects";
        mirrorExitMasked = false;
    }

    private void FinishMirrorExit()
    {
        RestoreObjectsLayer();
        ResetLifeState();
    }

    public bool IsMutant => data.IsMutant;

    protected virtual bool CanMutate => true;

    public void Mutate()
    {
        if (!CanMutate || data.IsMutant || IsMerging || !gameObject.activeInHierarchy)
        {
            return;
        }

        if (mutationData == null || mutationData.MutatedChickPrefab == null)
        {
            return;
        }

        SpawnManager.Instance.SpawnChick(mutationData.MutatedChickPrefab, transform.position, transform.parent);
        DespawnSelf();
    }

    private void TickContagion(float dt)
    {
        if (mutationData == null)
        {
            return;
        }

        contagionTimer -= dt;
        if (contagionTimer > 0f)
        {
            return;
        }

        contagionTimer = NextContagionInterval(mutationData);

        if (!contagionActive)
        {
            return;
        }

        SpreadContagion(mutationData);
    }

    private void SpreadContagion(MutationData mutation)
    {
        float radiusSqr = mutation.ContagionRadius * mutation.ContagionRadius;

        contagionBuffer.Clear();
        PlaceableEntity.Spatial.Query(transform.position, mutation.ContagionRadius, contagionBuffer);

        for (int i = contagionBuffer.Count - 1; i >= 0; i--)
        {
            if (!(contagionBuffer[i] is Chick other) || other == this || other.IsMutant)
            {
                continue;
            }

            if (other.IsCarried || other.IsMirrorExiting || other.IsMerging)
            {
                continue;
            }

            if ((other.transform.position - transform.position).sqrMagnitude > radiusSqr)
            {
                continue;
            }

            if (Random.value <= mutation.ContagionChance)
            {
                other.Mutate();
            }
        }
    }
}

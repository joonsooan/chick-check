using System.Collections.Generic;
using UnityEngine;

public class Chick : PlaceableEntity
{
    private static readonly List<PlaceableEntity> contagionBuffer = new List<PlaceableEntity>();

    [SerializeField] private ChickData data;
    [SerializeField] private MutationData mutationData;

    private float contagionTimer;
    private bool contagionActive;

    private readonly MirrorExitMotion exitMotion = new MirrorExitMotion();
    private bool exitMasked;
    private bool exitHasMaskBounds;
    private Bounds exitMaskBounds;

    public override EconomyEntityData EconomyData => data;
    public override bool CanBeCarried => base.CanBeCarried && !IsExiting;

    public float CurrentAlternateStageChance { get; private set; }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        exitMotion.Cancel();
        spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
        spriteRenderer.sortingLayerName = "Objects";
        ResetLifeState();
    }

    private void ResetLifeState()
    {
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

        exitMotion.Cancel();
    }

    public override void RefreshStats()
    {
        base.RefreshStats();

        UpgradeManager manager = UpgradeManager.Instance;
        contagionActive = manager != null && manager.State.ContagionEnabled;

        CurrentAlternateStageChance = ApplyModifier(ShopStatType.DuckGrowthChance, data.AlternateStageChance);
    }

    protected override GameObject PickNextStagePrefab(EconomyEntityData economyData)
    {
        if (data.AlternateStagePrefab != null && CurrentAlternateStageChance > 0f &&
            Random.value <= CurrentAlternateStageChance)
        {
            return data.AlternateStagePrefab;
        }

        return base.PickNextStagePrefab(economyData);
    }

    protected override void TickBehaviour(float dt)
    {
        if (exitMotion.IsActive)
        {
            TickExit(dt);
            return;
        }

        TickWalk(data, dt);
        TickAmbient(AmbientSoundKind.Chick, dt);
    }

    protected override void SlowTickBehaviour(float dt)
    {
        if (exitMotion.IsActive)
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
        return spawn == null || ChickGrowthRule.CanGrow(data.IsMutant, spawn.ChickCount, spawn.MutantChickCount);
    }

    public bool IsExiting => exitMotion.IsActive;

    public void ForceExit(Vector2 dir, MirrorData mirrorData, bool masked)
    {
        BeginExit(dir, mirrorData, masked, false, default);
    }

    public void ForceExit(Vector2 dir, MirrorData mirrorData, bool masked, Bounds maskBounds)
    {
        BeginExit(dir, mirrorData, masked, true, maskBounds);
    }

    private void BeginExit(Vector2 dir, MirrorData mirrorData, bool masked, bool hasMaskBounds, Bounds maskBounds)
    {
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

        exitMasked = masked;
        exitHasMaskBounds = hasMaskBounds;
        exitMaskBounds = maskBounds;
        exitMotion.Begin(
            transform.position,
            dir,
            data.MoveSpeed * exitMultiplier,
            mirrorData.ForcedExitDistance,
            mirrorData.RandomExitAngle,
            mirrorData.RandomExitDuration);

        spriteRenderer.flipX = exitMotion.Direction.x > 0f;
    }

    private void TickExit(float dt)
    {
        Vector3 position = transform.position;
        bool directionChanged = exitMotion.Tick(dt, ref position);
        transform.position = position;
        SyncSpatialCell();

        if (directionChanged)
        {
            spriteRenderer.flipX = exitMotion.Direction.x > 0f;
        }

        if (exitMasked && exitHasMaskBounds && !exitMaskBounds.Intersects(spriteRenderer.bounds))
        {
            RestoreObjectsLayer();
        }

        if (!exitMotion.IsActive)
        {
            FinishExit();
        }
    }

    private void RestoreObjectsLayer()
    {
        if (!exitMasked)
        {
            return;
        }

        spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
        spriteRenderer.sortingLayerName = "Objects";
        exitMasked = false;
    }

    private void FinishExit()
    {
        RestoreObjectsLayer();
        ResetLifeState();
    }

    public bool IsMutant => data.IsMutant;

    public void Mutate()
    {
        if (data.IsMutant || !gameObject.activeInHierarchy)
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

            if (other.IsCarried || other.IsExiting)
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

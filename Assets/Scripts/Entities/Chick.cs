using System.Collections.Generic;
using UnityEngine;

public class Chick : PlaceableEntity
{
    private enum ExitPhase
    {
        None,
        Forced,
        Random
    }

    private static readonly List<PlaceableEntity> contagionBuffer = new List<PlaceableEntity>();

    [SerializeField] private ChickData data;
    [SerializeField] private MutationData mutationData;

    private float contagionTimer;
    private bool contagionActive;

    private ExitPhase exitPhase = ExitPhase.None;
    private Vector3 exitTarget;
    private Vector2 exitDir;
    private float exitMoveSpeed;
    private float exitRandomAngleRange;
    private float exitRandomDuration;
    private float exitRandomElapsed;
    private bool exitMasked;

    public override EconomyEntityData EconomyData => data;
    public override bool CanBeCarried => base.CanBeCarried && !IsExiting;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        exitPhase = ExitPhase.None;
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

        exitPhase = ExitPhase.None;
    }

    public override void RefreshStats()
    {
        base.RefreshStats();

        UpgradeManager manager = UpgradeManager.Instance;
        contagionActive = manager != null && manager.State.ContagionEnabled;
    }

    protected override void TickBehaviour(float dt)
    {
        if (exitPhase != ExitPhase.None)
        {
            TickExit(dt);
            return;
        }

        TickWalk(data, dt);
        TickAmbient(AmbientSoundKind.Chick, dt);

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
        return SpawnManager.Instance == null || SpawnManager.Instance.ChickCount > 1;
    }

    public bool IsExiting => exitPhase != ExitPhase.None;

    public void ForceExit(Vector2 dir, MirrorData mirrorData, bool masked)
    {
        Vector2 normalizedDir = dir.normalized;

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
        exitMoveSpeed = data.MoveSpeed * exitMultiplier;
        exitDir = normalizedDir;
        exitTarget = transform.position + (Vector3)(exitDir * mirrorData.ForcedExitDistance);
        exitRandomAngleRange = mirrorData.RandomExitAngle;
        exitRandomDuration = mirrorData.RandomExitDuration;

        spriteRenderer.flipX = exitDir.x > 0f;

        exitPhase = ExitPhase.Forced;
    }

    private void TickExit(float dt)
    {
        if (exitPhase == ExitPhase.Forced)
        {
            transform.position = Vector3.MoveTowards(transform.position, exitTarget, exitMoveSpeed * dt);

            if ((exitTarget - transform.position).sqrMagnitude <= 0.0001f)
            {
                float randomAngle = Random.Range(-exitRandomAngleRange, exitRandomAngleRange);
                exitDir = Quaternion.Euler(0f, 0f, randomAngle) * exitDir;
                spriteRenderer.flipX = exitDir.x > 0f;
                exitRandomElapsed = 0f;
                exitPhase = ExitPhase.Random;
            }

            return;
        }

        transform.position += (Vector3)(exitDir * exitMoveSpeed * dt);
        exitRandomElapsed += dt;

        if (exitRandomElapsed >= exitRandomDuration)
        {
            FinishExit();
        }
    }

    private void FinishExit()
    {
        spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
        if (exitMasked)
        {
            spriteRenderer.sortingLayerName = "Objects";
        }

        exitPhase = ExitPhase.None;
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

        if (Random.value > mutationData.ContagionChance)
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

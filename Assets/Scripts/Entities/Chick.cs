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

    [SerializeField] private ChickData data;
    [SerializeField] private MutationData mutationData;

    private Collider2D col;
    private bool colliderWasTrigger;

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

        col = GetComponent<Collider2D>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        exitPhase = ExitPhase.None;
        spriteRenderer.sprite = data.Sprite;
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
            contagionTimer = mutationData != null ? mutationData.ContagionInterval : 0f;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (exitPhase != ExitPhase.None)
        {
            RestoreCollider();
            exitPhase = ExitPhase.None;
        }
    }

    public override void RefreshStats()
    {
        base.RefreshStats();

        UpgradeManager manager = UpgradeManager.Instance;
        contagionActive = manager != null && manager.State.ContagionEnabled;
    }

    public override void Tick(float dt)
    {
        if (IsCarried)
        {
            return;
        }

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
            animator.SetBool(IsWalkingHash, false);
        }
        else
        {
            ResetLifeState();
        }
    }

    private void RestoreCollider()
    {
        if (col != null)
        {
            col.isTrigger = colliderWasTrigger;
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

        if (col != null)
        {
            colliderWasTrigger = col.isTrigger;
            col.isTrigger = true;
        }

        animator.SetBool(IsWalkingHash, true);

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

        RestoreCollider();
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
        PoolManager.Despawn(gameObject);
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

        contagionTimer = mutationData.ContagionInterval;

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
        IReadOnlyList<PlaceableEntity> entities = PlaceableEntity.ActiveEntities;

        for (int i = entities.Count - 1; i >= 0; i--)
        {
            if (!(entities[i] is Chick other) || other == this || other.IsMutant)
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

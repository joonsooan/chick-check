using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chick : PlaceableEntity
{
    [SerializeField] private ChickData data;

    private Collider2D col;
    private bool colliderWasTrigger;

    private float contagionTimer;

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

        IsExiting = false;
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
            MutationData mutationData = UpgradeManager.Instance != null ? UpgradeManager.Instance.MutationData : null;
            contagionTimer = mutationData != null ? mutationData.ContagionInterval : 1f;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        StopAllCoroutines();
    }

    public override void Tick(float dt)
    {
        if (IsCarried || IsExiting)
        {
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

    public override void NotifySpawnCount()
    {
        SpawnManager.Instance?.RegisterChickSpawned();
    }

    public override void NotifyDespawnCount()
    {
        SpawnManager.Instance?.RegisterChickDespawned();
    }

    public bool IsExiting { get; private set; }

    public void ForceExit(Vector2 dir, MirrorData mirrorData, bool masked)
    {
        StopAllCoroutines();
        StartCoroutine(ForceExitRoutine(dir.normalized, mirrorData, masked));
    }

    private IEnumerator ForceExitRoutine(Vector2 dir, MirrorData mirrorData, bool masked)
    {
        IsExiting = true;
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
        float moveSpeed = data.MoveSpeed * exitMultiplier;

        yield return MoveForDistance(dir, moveSpeed, mirrorData.ForcedExitDistance);

        float randomAngle = Random.Range(-mirrorData.RandomExitAngle, mirrorData.RandomExitAngle);
        Vector2 randomDir = Quaternion.Euler(0f, 0f, randomAngle) * dir;
        yield return MoveForDuration(randomDir, moveSpeed, mirrorData.RandomExitDuration);

        spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
        if (masked)
        {
            spriteRenderer.sortingLayerName = "Objects";
        }
        RestoreCollider();
        IsExiting = false;
        ResetLifeState();
    }

    public bool IsMutant => data.IsMutant;

    public void Mutate()
    {
        if (data.IsMutant || !gameObject.activeInHierarchy)
        {
            return;
        }

        MutationData mutationData = UpgradeManager.Instance != null ? UpgradeManager.Instance.MutationData : null;
        if (mutationData == null || mutationData.MutatedChickPrefab == null)
        {
            return;
        }

        NotifyDespawnCount();
        SpawnManager.Instance.SpawnChick(mutationData.MutatedChickPrefab, transform.position, transform.parent);
        PoolManager.Despawn(gameObject);
    }

    private void TickContagion(float dt)
    {
        MutationData mutationData = UpgradeManager.Instance != null ? UpgradeManager.Instance.MutationData : null;
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

        if (UpgradeManager.Instance == null || !UpgradeManager.Instance.ContagionEnabled)
        {
            return;
        }

        if (Random.value > mutationData.ContagionChance)
        {
            return;
        }

        SpreadContagion(mutationData);
    }

    private void SpreadContagion(MutationData mutationData)
    {
        float radiusSqr = mutationData.ContagionRadius * mutationData.ContagionRadius;
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

            if (Random.value <= mutationData.ContagionChance)
            {
                other.Mutate();
            }
        }
    }

    private IEnumerator MoveForDuration(Vector2 dir, float moveSpeed, float duration)
    {
        spriteRenderer.flipX = dir.x > 0f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator MoveForDistance(Vector2 dir, float moveSpeed, float distance)
    {
        spriteRenderer.flipX = dir.x > 0f;

        Vector3 target = transform.position + (Vector3)(dir * distance);
        while ((target - transform.position).sqrMagnitude > 0.0001f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }
}

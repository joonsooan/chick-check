using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlaceableEntity : MonoBehaviour
{
    protected static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");

    private static readonly List<PlaceableEntity> activeEntities = new List<PlaceableEntity>();

    public static IReadOnlyList<PlaceableEntity> ActiveEntities => activeEntities;

    protected SpriteRenderer spriteRenderer;
    protected Animator animator;

    public SpriteRenderer SpriteRenderer => spriteRenderer;
    public bool IsCarried { get; private set; }

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    protected virtual void OnEnable()
    {
        activeEntities.Add(this);
    }

    protected virtual void OnDisable()
    {
        activeEntities.Remove(this);
    }

    public virtual void SetCarried(bool carried)
    {
        IsCarried = carried;
    }

    protected IEnumerator GrowthRoutine(EconomyEntityData data)
    {
        if (!data.CanGrow)
        {
            yield break;
        }

        while (true)
        {
            yield return new WaitForSeconds(data.GrowthInterval);

            if (Random.value <= data.GrowthChance)
            {
                GrowToNextStage(data.NextStagePrefab);
                yield break;
            }
        }
    }

    private void GrowToNextStage(GameObject nextStagePrefab)
    {
        GameObject next = PoolManager.Instance.Spawn(nextStagePrefab, transform.position, transform.rotation, transform.parent);
        next.GetComponent<PlaceableEntity>()?.NotifySpawnCount();
        NotifyDespawnCount();
        PoolManager.Despawn(gameObject);
    }

    protected virtual void NotifySpawnCount()
    {
    }

    protected virtual void NotifyDespawnCount()
    {
    }

    protected IEnumerator WalkRoutine(EconomyEntityData data)
    {
        while (true)
        {
            if (animator != null)
            {
                animator.SetBool(IsWalkingHash, false);
            }

            Vector2 interval = data.WalkIntervalRange;
            yield return new WaitForSeconds(Random.Range(interval.x, interval.y));

            if (!TryPickWalkTarget(data, out Vector3 target))
            {
                continue;
            }

            spriteRenderer.flipX = target.x > transform.position.x;
            if (animator != null)
            {
                animator.SetBool(IsWalkingHash, true);
            }

            while ((target - transform.position).sqrMagnitude > 0.0001f)
            {
                Vector3 next = Vector3.MoveTowards(
                    transform.position, target, data.MoveSpeed * Time.deltaTime);

                if (GridManager.Instance != null && !GridManager.Instance.IsWalkable(next))
                {
                    break;
                }

                transform.position = next;
                yield return null;
            }
        }
    }

    private bool TryPickWalkTarget(EconomyEntityData data, out Vector3 target)
    {
        Vector2 distance = data.WalkDistanceRange;
        for (int attempt = 0; attempt < 10; attempt++)
        {
            Vector2 dir = Random.insideUnitCircle.normalized;
            Vector3 candidate = transform.position + (Vector3)(dir * Random.Range(distance.x, distance.y));

            if (GridManager.Instance == null || GridManager.Instance.IsWalkable(candidate))
            {
                target = candidate;
                return true;
            }
        }

        target = transform.position;
        return false;
    }
}

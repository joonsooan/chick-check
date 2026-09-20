using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlaceableEntity : MonoBehaviour
{
    protected static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");

    private const int CarriedSortingOrder = 100;

    private static readonly List<PlaceableEntity> activeEntities = new List<PlaceableEntity>();

    public static IReadOnlyList<PlaceableEntity> ActiveEntities => activeEntities;

    protected SpriteRenderer spriteRenderer;
    protected Animator animator;

    private int defaultSortingOrder;

    public SpriteRenderer SpriteRenderer => spriteRenderer;
    public bool IsCarried { get; private set; }
    public virtual bool CanBeCarried => true;
    public abstract EconomyEntityData EconomyData { get; }

    public float CurrentGrowthInterval { get; private set; }
    public float CurrentGrowthChance { get; private set; }

    private float growthElapsed;

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        defaultSortingOrder = spriteRenderer.sortingOrder;
    }

    protected virtual void OnEnable()
    {
        activeEntities.Add(this);
        growthElapsed = 0f;

        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnModifiersChanged += RefreshDebugStats;
        }
        RefreshDebugStats();
    }

    protected virtual void OnDisable()
    {
        activeEntities.Remove(this);

        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnModifiersChanged -= RefreshDebugStats;
        }
    }

    protected virtual void RefreshDebugStats()
    {
        EconomyEntityData data = EconomyData;
        if (data == null)
        {
            return;
        }

        CurrentGrowthInterval = data.GrowthInterval;
        CurrentGrowthChance = ApplyModifier(GrowthChanceStat, data.GrowthChance);
    }

    protected virtual ShopStatType GrowthChanceStat => ShopStatType.GrowthChance;

    public virtual ShopStatType SellPriceStat => ShopStatType.SellPriceChick;

    protected static float ApplyModifier(ShopStatType stat, float baseValue)
    {
        return UpgradeManager.Instance != null
            ? UpgradeManager.Instance.ApplyModifiers(stat, baseValue)
            : baseValue;
    }

    public virtual void SetCarried(bool carried)
    {
        IsCarried = carried;
        spriteRenderer.sortingOrder = carried ? CarriedSortingOrder : defaultSortingOrder;
    }

    protected IEnumerator GrowthRoutine(EconomyEntityData data)
    {
        if (!data.CanGrow)
        {
            yield break;
        }

        while (true)
        {
            CurrentGrowthInterval = data.GrowthInterval;
            CurrentGrowthChance = ApplyModifier(GrowthChanceStat, data.GrowthChance);

            while (growthElapsed < CurrentGrowthInterval)
            {
                growthElapsed += Time.deltaTime;
                yield return null;
            }

            growthElapsed = 0f;

            if (Random.value <= CurrentGrowthChance)
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

    public virtual void NotifySpawnCount()
    {
    }

    public virtual void NotifyDespawnCount()
    {
    }

    public void HandleSold()
    {
        NotifyDespawnCount();
        PoolManager.Despawn(gameObject);
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

            float moveSpeed = data.MoveSpeed;

            while ((target - transform.position).sqrMagnitude > 0.0001f)
            {
                Vector3 next = Vector3.MoveTowards(
                    transform.position, target, moveSpeed * Time.deltaTime);

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
        Vector3 origin = transform.position;

        Vector3 RandomCandidate(int attempt)
        {
            Vector2 dir = Random.insideUnitCircle.normalized;
            return origin + (Vector3)(dir * Random.Range(distance.x, distance.y));
        }

        if (GridManager.Instance == null)
        {
            target = RandomCandidate(0);
            return true;
        }

        return GridManager.Instance.TryFindWalkable(origin, 10, RandomCandidate, out target);
    }
}

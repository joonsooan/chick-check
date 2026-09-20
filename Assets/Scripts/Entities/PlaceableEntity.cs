using System.Collections.Generic;
using UnityEngine;

public abstract class PlaceableEntity : MonoBehaviour
{
    protected static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");

    private const int CarriedSortingOrder = 100;

    private static readonly List<PlaceableEntity> activeEntities = new List<PlaceableEntity>();

    public static IReadOnlyList<PlaceableEntity> ActiveEntities => activeEntities;

    protected enum WalkState
    {
        Waiting,
        Moving
    }

    protected SpriteRenderer spriteRenderer;
    protected Animator animator;

    private int defaultSortingOrder;

    private int activeIndex = -1;

    public SpriteRenderer SpriteRenderer => spriteRenderer;
    public bool IsCarried { get; private set; }
    public virtual bool CanBeCarried => true;
    public abstract EconomyEntityData EconomyData { get; }

    public float CurrentGrowthInterval { get; private set; }
    public float CurrentGrowthChance { get; private set; }

    private float growthElapsed;

    private WalkState walkState;
    private float walkTimer;
    private Vector3 walkTarget;

    private float ambientTimer;

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        defaultSortingOrder = spriteRenderer.sortingOrder;
    }

    protected virtual void OnEnable()
    {
        activeIndex = activeEntities.Count;
        activeEntities.Add(this);

        growthElapsed = 0f;
        RefreshDebugStats();
    }

    protected virtual void OnDisable()
    {
        RemoveFromActive();
    }

    private void RemoveFromActive()
    {
        int last = activeEntities.Count - 1;
        if (activeIndex < 0 || activeIndex > last)
        {
            activeIndex = -1;
            return;
        }

        PlaceableEntity moved = activeEntities[last];
        activeEntities[activeIndex] = moved;
        moved.activeIndex = activeIndex;
        activeEntities.RemoveAt(last);
        activeIndex = -1;
    }

    public virtual void Tick(float dt)
    {
        if (IsCarried)
        {
            return;
        }

        TickGrowth(dt);
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

    protected virtual bool CanGrowNow() => true;

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

        if (carried)
        {
            growthElapsed = 0f;
        }
    }

    protected void TickGrowth(float dt)
    {
        EconomyEntityData data = EconomyData;
        if (data == null || !data.CanGrow)
        {
            return;
        }

        CurrentGrowthInterval = data.GrowthInterval;
        CurrentGrowthChance = ApplyModifier(GrowthChanceStat, data.GrowthChance);

        growthElapsed += dt;
        if (growthElapsed < CurrentGrowthInterval)
        {
            return;
        }

        growthElapsed = 0f;

        if (Random.value <= CurrentGrowthChance)
        {
            if (!CanGrowNow())
            {
                return;
            }

            GrowToNextStage(data.NextStagePrefab);
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

    protected void ResetWalk(EconomyEntityData data)
    {
        if (animator != null)
        {
            animator.SetBool(IsWalkingHash, false);
        }

        walkState = WalkState.Waiting;
        Vector2 interval = data.WalkIntervalRange;
        walkTimer = Random.Range(interval.x, interval.y);
    }

    protected void TickWalk(EconomyEntityData data, float dt)
    {
        if (walkState == WalkState.Waiting)
        {
            walkTimer -= dt;
            if (walkTimer > 0f)
            {
                return;
            }

            if (!TryPickWalkTarget(data, out walkTarget))
            {
                Vector2 interval = data.WalkIntervalRange;
                walkTimer = Random.Range(interval.x, interval.y);
                return;
            }

            spriteRenderer.flipX = walkTarget.x > transform.position.x;
            if (animator != null)
            {
                animator.SetBool(IsWalkingHash, true);
            }

            walkState = WalkState.Moving;
            return;
        }

        float moveSpeed = data.MoveSpeed;
        Vector3 next = Vector3.MoveTowards(transform.position, walkTarget, moveSpeed * dt);

        if (GridManager.Instance != null && !GridManager.Instance.IsWalkable(next))
        {
            ResetWalk(data);
            return;
        }

        transform.position = next;

        if ((walkTarget - transform.position).sqrMagnitude <= 0.0001f)
        {
            ResetWalk(data);
        }
    }

    protected void ResetAmbient(AmbientSoundKind kind)
    {
        Vector2 range = AudioManager.Instance != null
            ? AudioManager.Instance.AmbientIntervalRange(kind)
            : new Vector2(4f, 9f);
        ambientTimer = Random.Range(range.x, range.y);
    }

    protected void TickAmbient(AmbientSoundKind kind, float dt)
    {
        ambientTimer -= dt;
        if (ambientTimer > 0f)
        {
            return;
        }

        Vector2 range = AudioManager.Instance != null
            ? AudioManager.Instance.AmbientIntervalRange(kind)
            : new Vector2(4f, 9f);
        ambientTimer = Random.Range(range.x, range.y);

        if (!IsCarried)
        {
            AudioManager.Instance?.PlayAmbient(kind);
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

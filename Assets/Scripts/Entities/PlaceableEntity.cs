using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlaceableEntity : MonoBehaviour
{
    protected static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");

    private const int CarriedSortingOrder = 100;

    private static readonly List<PlaceableEntity> activeEntities = new List<PlaceableEntity>();

    public static IReadOnlyList<PlaceableEntity> ActiveEntities => activeEntities;
    public static event Action<PlaceableEntity, bool> OnActiveChanged;

    protected SpriteRenderer spriteRenderer;
    protected Animator animator;

    private EntityWalker walker;
    private AmbientVoice ambientVoice;

    private int defaultSortingOrder;

    private int activeIndex = -1;

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
        walker = new EntityWalker(transform, spriteRenderer, animator, IsWalkable);
        ambientVoice = new AmbientVoice();
    }

    protected virtual void OnEnable()
    {
        activeIndex = activeEntities.Count;
        activeEntities.Add(this);

        growthElapsed = 0f;
        RefreshStats();

        OnActiveChanged?.Invoke(this, true);
    }

    protected virtual void OnDisable()
    {
        RemoveFromActive();

        OnActiveChanged?.Invoke(this, false);
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

    public virtual void RefreshStats()
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
        else
        {
            RefreshStats();
        }
    }

    protected void TickGrowth(float dt)
    {
        EconomyEntityData data = EconomyData;
        if (data == null || !data.CanGrow)
        {
            return;
        }

        growthElapsed += dt;
        if (growthElapsed < CurrentGrowthInterval)
        {
            return;
        }

        growthElapsed = 0f;

        if (UnityEngine.Random.value <= CurrentGrowthChance)
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
        PoolManager.Instance.Spawn(nextStagePrefab, transform.position, transform.rotation, transform.parent);
        PoolManager.Despawn(gameObject);
    }

    public void HandleSold()
    {
        PoolManager.Despawn(gameObject);
    }

    private static bool IsWalkable(Vector3 position)
    {
        return GridManager.Instance == null || GridManager.Instance.IsWalkable(position);
    }

    protected void ResetWalk(EconomyEntityData data)
    {
        walker.Reset(data);
    }

    protected void TickWalk(EconomyEntityData data, float dt)
    {
        walker.Tick(data, dt);
    }

    protected void ResetAmbient(AmbientSoundKind kind)
    {
        ambientVoice.Reset(kind);
    }

    protected void TickAmbient(AmbientSoundKind kind, float dt)
    {
        ambientVoice.Tick(kind, dt, IsCarried);
    }
}

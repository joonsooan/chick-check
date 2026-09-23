using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlaceableEntity : MonoBehaviour
{
    private const int CarriedSortingOrder = 100;
    private const float SpatialCellSize = 1f;

    private static readonly List<PlaceableEntity> activeEntities = new List<PlaceableEntity>();

    public static IReadOnlyList<PlaceableEntity> ActiveEntities => activeEntities;
    public static event Action<PlaceableEntity, bool> OnActiveChanged;

    public static SpatialHash<PlaceableEntity> Spatial { get; } = new SpatialHash<PlaceableEntity>(SpatialCellSize);

    protected SpriteRenderer spriteRenderer;
    protected SpriteFlipbook flipbook;

    private EntityWalker walker;
    private AmbientVoice ambientVoice;
    private PooledObject pooledObject;

    private int defaultSortingOrder;

    private int activeIndex = -1;
    private Vector2Int currentCell;

    private float lastSlowTime;

    public SpriteRenderer SpriteRenderer => spriteRenderer;
    public bool IsActiveEntity => activeIndex >= 0;
    public bool IsCarried { get; private set; }
    public virtual bool CanBeCarried => true;
    public abstract EconomyEntityData EconomyData { get; }

    public float CurrentGrowthInterval { get; private set; }
    public float CurrentGrowthChance { get; private set; }

    private float growthElapsed;

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        flipbook = new SpriteFlipbook(spriteRenderer);
        defaultSortingOrder = spriteRenderer.sortingOrder;
        walker = new EntityWalker(transform, spriteRenderer, flipbook, IsWalkable, SyncSpatialCell);
        ambientVoice = new AmbientVoice();
        pooledObject = GetComponent<PooledObject>();
    }

    protected virtual void OnEnable()
    {
        activeIndex = activeEntities.Count;
        activeEntities.Add(this);

        Spatial.Insert(this, transform.position);
        currentCell = Spatial.CellOf(transform.position);

        ResetGrowthTimer();
        RefreshStats();
        ApplyIdleVisual();

        lastSlowTime = Time.time;

        OnActiveChanged?.Invoke(this, true);
    }

    private void ApplyIdleVisual()
    {
        EconomyEntityData data = EconomyData;
        if (data == null)
        {
            return;
        }

        spriteRenderer.sprite = data.Sprite;
        flipbook.Stop();
        flipbook.Play(data.IdleAnimation);
    }

    protected virtual void OnDisable()
    {
        RemoveFromActive();

        Spatial.Remove(this);

        OnActiveChanged?.Invoke(this, false);
    }

    public void SyncSpatialCell()
    {
        if (Spatial.Move(this, currentCell, transform.position, out Vector2Int newCell))
        {
            currentCell = newCell;
        }
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

    public void Tick(float dt)
    {
        flipbook.Tick(dt);

        if (IsCarried)
        {
            return;
        }

        TickBehaviour(dt);
    }

    protected virtual void TickBehaviour(float dt)
    {
    }

    public void SlowTick(float now)
    {
        float dt = now - lastSlowTime;
        lastSlowTime = now;

        if (IsCarried)
        {
            return;
        }

        SlowTickBehaviour(dt);
    }

    protected virtual void SlowTickBehaviour(float dt)
    {
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
            ResetGrowthTimer();
        }
        else
        {
            RefreshStats();
        }
    }

    private void ResetGrowthTimer()
    {
        EconomyEntityData data = EconomyData;
        growthElapsed = data != null ? -RandomJitter(data.GrowthTimerJitterRange) : 0f;
    }

    private static float RandomJitter(Vector2 range)
    {
        return UnityEngine.Random.Range(range.x, range.y);
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

        ResetGrowthTimer();

        if (CurrentGrowthChance <= 0f)
        {
            return;
        }

        if (UnityEngine.Random.value <= CurrentGrowthChance)
        {
            if (!CanGrowNow())
            {
                return;
            }

            GrowToNextStage(PickNextStagePrefab(data));
        }
    }

    protected virtual GameObject PickNextStagePrefab(EconomyEntityData data) => data.NextStagePrefab;

    private void GrowToNextStage(GameObject nextStagePrefab)
    {
        if (nextStagePrefab == null)
        {
            return;
        }

        PoolManager.Instance.Spawn(nextStagePrefab, transform.position, transform.rotation, transform.parent);
        DespawnSelf();
    }

    public void HandleSold()
    {
        DespawnSelf();
    }

    protected void DespawnSelf()
    {
        if (pooledObject == null)
        {
            TryGetComponent(out pooledObject);
        }

        if (pooledObject != null)
        {
            PoolManager.Despawn(pooledObject);
        }
        else
        {
            PoolManager.Despawn(gameObject);
        }
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

using System;
using System.Collections.Generic;
using DG.Tweening;
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
    private Vector3 defaultScale;

    private int activeIndex = -1;
    private Vector2Int currentCell;

    private float lastSlowTime;

    public Vector3 DefaultScale => defaultScale;

    public GameObject SourcePrefab
    {
        get
        {
            if (pooledObject == null)
            {
                TryGetComponent(out pooledObject);
            }

            return pooledObject != null && pooledObject.Pool != null ? pooledObject.Pool.Prefab : null;
        }
    }

    public SpriteRenderer SpriteRenderer => spriteRenderer;
    public bool IsRegistered => activeIndex >= 0;
    public bool IsCarried { get; private set; }
    public bool IsMerging { get; private set; }
    public virtual bool CanBeCarried => !IsMerging;
    public abstract EconomyEntityData EconomyData { get; }

    public float CurrentGrowthInterval { get; private set; }
    public float CurrentGrowthChance { get; private set; }

    private float growthElapsed;

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        flipbook = new SpriteFlipbook(spriteRenderer);
        defaultSortingOrder = spriteRenderer.sortingOrder;
        defaultScale = transform.localScale;
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

        transform.localScale = defaultScale;

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
        transform.DOKill();

        IsMerging = false;

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

        if (IsCarried || IsMerging)
        {
            return;
        }

        TickBehaviour(dt);
    }

    protected virtual void TickBehaviour(float dt)
    {
    }

    public void SlowTick(float time)
    {
        float dt = time - lastSlowTime;
        lastSlowTime = time;

        if (IsCarried || IsMerging)
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

    protected virtual ShopStatType GrowthChanceStat => ShopStatType.ChickGrowthChance;

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
        if (data == null || !data.HasNextStage)
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

    protected virtual void GrowToNextStage(GameObject nextStagePrefab)
    {
        if (nextStagePrefab == null)
        {
            return;
        }

        SpawnNextStage(nextStagePrefab);
        DespawnSelf();
    }

    protected GameObject SpawnNextStage(GameObject nextStagePrefab)
    {
        GameObject spawned = PoolManager.Instance.Spawn(
            nextStagePrefab, transform.position, transform.rotation, transform.parent);

        if (spawned != null && spawned.TryGetComponent(out PlaceableEntity entity))
        {
            entity.PlayGrowthPop();
        }

        return spawned;
    }

    public void PlayPunchScale(Vector3 punch, float duration, int vibrato = 10, float elasticity = 1f)
    {
        if (duration <= 0f)
        {
            return;
        }

        transform.DOKill();
        transform.localScale = defaultScale;
        transform.DOPunchScale(punch, duration, vibrato, elasticity);
    }

    public void PlayGrowthPop()
    {
        EconomyEntityData data = EconomyData;
        if (data == null || data.GrowthPopDuration <= 0f)
        {
            return;
        }

        transform.DOKill();
        transform.localScale = defaultScale;
        transform.DOPunchScale(
            data.GrowthPopPunch, data.GrowthPopDuration, data.GrowthPopVibrato, data.GrowthPopElasticity);
    }

    public void BeginMerge(Vector3 target, float duration, Ease ease)
    {
        IsMerging = true;

        EconomyEntityData data = EconomyData;
        if (data != null)
        {
            flipbook.Stop();
            flipbook.Play(data.WalkAnimation);
        }

        spriteRenderer.flipX = target.x > transform.position.x;

        transform.DOKill();
        transform
            .DOMove(target, duration)
            .SetEase(ease)
            .OnUpdate(SyncSpatialCell)
            .OnComplete(PlayIdleAfterMerge);

        OnMergeBegin();
    }

    private void PlayIdleAfterMerge()
    {
        EconomyEntityData data = EconomyData;
        if (IsMerging && data != null)
        {
            flipbook.Play(data.IdleAnimation);
        }
    }

    public void CancelMerge()
    {
        if (!IsMerging)
        {
            return;
        }

        transform.DOKill();
        IsMerging = false;

        OnMergeCancel();
    }

    protected virtual void OnMergeBegin()
    {
    }

    protected virtual void OnMergeCancel()
    {
    }

    public void Despawn()
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

    protected void WalkTo(Vector3 destination, EconomyEntityData data, bool ignoreObstacles = false)
    {
        walker.WalkTo(destination, data, ignoreObstacles);
    }

    protected bool IsWalkMoving => walker.IsMoving;

    protected void ResetAmbient(AmbientSoundKind kind)
    {
        ambientVoice.Reset(kind);
    }

    protected void TickAmbient(AmbientSoundKind kind, float dt)
    {
        ambientVoice.Tick(kind, dt, IsCarried);
    }
}

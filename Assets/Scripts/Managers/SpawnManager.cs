using System;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : Singleton<SpawnManager>
{
    [SerializeField] private Chick chickPrefab;
    [SerializeField] private RoundConfigData roundConfig;
    [SerializeField] private Vector3 chickSpawnCenter;

    private readonly System.Random rng = new System.Random();

    private readonly Dictionary<EntityKind, int> kindCounts = new Dictionary<EntityKind, int>();
    public event Action<EntityKind, int> OnKindCountChanged;

    public int ChickCount => CountOf(EntityKind.Chick);
    public int MutantChickCount { get; private set; }

    private readonly Dictionary<EconomyEntityData, int> entityCounts = new Dictionary<EconomyEntityData, int>();
    public event Action<EconomyEntityData, int> OnEntityCountChanged;

    public Chick DefaultChickPrefab => chickPrefab;

    public Chick DefaultMutantChickPrefab => chickPrefab != null ? chickPrefab.MutatedPrefab : null;

    public int CountOf(EntityKind kind)
    {
        kindCounts.TryGetValue(kind, out int count);
        return count;
    }

    public int CountOf(EconomyEntityData data)
    {
        if (data == null)
        {
            return 0;
        }

        entityCounts.TryGetValue(data, out int count);
        return count;
    }

    public int CountOf(IReadOnlyList<EconomyEntityData> dataList)
    {
        int sum = 0;
        for (int i = 0; i < dataList.Count; i++)
        {
            sum += CountOf(dataList[i]);
        }

        return sum;
    }

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this)
        {
            return;
        }

        if (roundConfig == null)
        {
            Debug.LogError("SpawnManager.roundConfig is not assigned.", this);
        }

        PlaceableEntity.OnActiveChanged += HandleActiveChanged;

        IReadOnlyList<PlaceableEntity> existing = PlaceableEntity.ActiveEntities;
        for (int i = 0; i < existing.Count; i++)
        {
            HandleActiveChanged(existing[i], true);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        PlaceableEntity.OnActiveChanged -= HandleActiveChanged;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundSpawnRequested -= HandleRoundSpawnRequested;
        }
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundSpawnRequested += HandleRoundSpawnRequested;
        }
    }

    private void HandleRoundSpawnRequested(int roundNumber)
    {
        SpawnRoundChicks();
    }

    private void SpawnRoundChicks()
    {
        int spawnCount = roundConfig.ChickSpawnCountPerRound;
        if (UpgradeManager.Instance != null)
        {
            spawnCount = Mathf.RoundToInt(
                UpgradeManager.Instance.ApplyModifiers(ShopStatType.ChickSpawnCountPerRound, spawnCount));
        }

        int mutantCount = 0;
        if (UpgradeManager.Instance != null)
        {
            mutantCount = Mathf.Min(UpgradeManager.Instance.State.Get<MutationState>().MutantSpawnCountPerRound, spawnCount);
        }

        SpawnChicksInRing(spawnCount, mutantCount, DefaultMutantChickPrefab);
    }

    public void SpawnDebugChicks(int count)
    {
        SpawnChicksInRing(count, 0, null);
    }

    private void SpawnChicksInRing(int spawnCount, int mutantCount, Chick mutantPrefab)
    {
        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 spawnPosition = SpawnPointSampler.Sample(
                chickSpawnCenter,
                roundConfig.ChickSpawnOuterRadius,
                roundConfig.ChickSpawnInnerRadius,
                roundConfig.ChickSpawnJitter,
                rng);

            if (i < mutantCount && mutantPrefab != null)
            {
                SpawnChick(mutantPrefab, spawnPosition);
            }
            else
            {
                SpawnChick(spawnPosition);
            }
        }
    }

    public Chick SpawnChick(Vector3 position, Transform parent = null)
    {
        return SpawnChick(chickPrefab, position, parent);
    }

    public Chick SpawnChick(Chick prefab, Vector3 position, Transform parent = null)
    {
        return PoolManager.Instance.Spawn(prefab, position, Quaternion.identity, parent);
    }

    private void HandleActiveChanged(PlaceableEntity entity, bool active)
    {
        EconomyEntityData data = entity != null ? entity.EconomyData : null;
        if (data == null)
        {
            return;
        }

        int delta = active ? 1 : -1;

        if (data.Kind == EntityKind.Chick && entity is Chick chick && chick.IsMutant)
        {
            MutantChickCount += delta;
        }

        int kindCount = CountOf(data.Kind) + delta;
        kindCounts[data.Kind] = kindCount;
        OnKindCountChanged?.Invoke(data.Kind, kindCount);

        int entityCount = CountOf(data) + delta;
        entityCounts[data] = entityCount;
        OnEntityCountChanged?.Invoke(data, entityCount);
    }
}

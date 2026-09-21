using System;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : Singleton<SpawnManager>
{
    [SerializeField] private Chick chickPrefab;
    [SerializeField] private RoundConfigData roundConfig;
    [SerializeField] private Vector3 chickSpawnCenter;

    private readonly System.Random rng = new System.Random();

    public int ChickCount { get; private set; }
    public event Action<int> OnChickCountChanged;

    public int EggCount { get; private set; }
    public event Action<int> OnEggCountChanged;

    public int ChickenCount { get; private set; }
    public event Action<int> OnChickenCountChanged;

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
        Chick mutantPrefab = null;
        if (UpgradeManager.Instance != null && UpgradeManager.Instance.MutationData != null)
        {
            mutantPrefab = UpgradeManager.Instance.MutationData.MutatedChickPrefab;
            mutantCount = Mathf.Min(UpgradeManager.Instance.State.MutantSpawnCount, spawnCount);
        }

        SpawnChicksInRing(spawnCount, mutantCount, mutantPrefab);
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

    public void DespawnChick(Chick chick)
    {
        if (chick.TryGetComponent(out PooledObject pooled))
        {
            PoolManager.Despawn(pooled);
        }
        else
        {
            PoolManager.Despawn(chick.gameObject);
        }
    }

    private void HandleActiveChanged(PlaceableEntity entity, bool active)
    {
        EconomyEntityData data = entity != null ? entity.EconomyData : null;
        if (data == null)
        {
            return;
        }

        switch (data.Kind)
        {
            case EntityKind.Chick:
                ChickCount += active ? 1 : -1;
                OnChickCountChanged?.Invoke(ChickCount);
                break;
            case EntityKind.Egg:
                EggCount += active ? 1 : -1;
                OnEggCountChanged?.Invoke(EggCount);
                break;
            case EntityKind.Chicken:
                ChickenCount += active ? 1 : -1;
                OnChickenCountChanged?.Invoke(ChickenCount);
                break;
        }
    }
}

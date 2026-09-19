using System;
using UnityEngine;

public class SpawnManager : Singleton<SpawnManager>
{
    [SerializeField] private Chick chickPrefab;

    public int ChickCount { get; private set; }
    public event Action<int> OnChickCountChanged;

    public int EggCount { get; private set; }
    public event Action<int> OnEggCountChanged;

    public int ChickenCount { get; private set; }
    public event Action<int> OnChickenCountChanged;

    public Chick SpawnChick(Vector3 position, Transform parent = null)
    {
        Chick chick = PoolManager.Instance.Spawn(chickPrefab, position, Quaternion.identity, parent);
        RegisterChickSpawned();
        return chick;
    }

    public void DespawnChick(Chick chick)
    {
        PoolManager.Despawn(chick.gameObject);
        RegisterChickDespawned();
    }

    public void RegisterChickSpawned()
    {
        ChickCount++;
        OnChickCountChanged?.Invoke(ChickCount);
    }

    public void RegisterChickDespawned()
    {
        ChickCount--;
        OnChickCountChanged?.Invoke(ChickCount);
    }

    public void RegisterEggSpawned()
    {
        EggCount++;
        OnEggCountChanged?.Invoke(EggCount);
    }

    public void RegisterEggDespawned()
    {
        EggCount--;
        OnEggCountChanged?.Invoke(EggCount);
    }

    public void RegisterChickenSpawned()
    {
        ChickenCount++;
        OnChickenCountChanged?.Invoke(ChickenCount);
    }

    public void RegisterChickenDespawned()
    {
        ChickenCount--;
        OnChickenCountChanged?.Invoke(ChickenCount);
    }
}

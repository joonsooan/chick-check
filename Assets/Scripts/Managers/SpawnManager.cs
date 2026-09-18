using System;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [SerializeField] private Chick chickPrefab;

    public int ChickCount { get; private set; }
    public event Action<int> OnChickCountChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public Chick SpawnChick(Vector3 position, Transform parent = null)
    {
        Chick chick = PoolManager.Instance.Spawn(chickPrefab, position, Quaternion.identity, parent);
        ChickCount++;
        OnChickCountChanged?.Invoke(ChickCount);
        return chick;
    }

    public void DespawnChick(Chick chick)
    {
        PoolManager.Despawn(chick.gameObject);
        ChickCount--;
        OnChickCountChanged?.Invoke(ChickCount);
    }
}

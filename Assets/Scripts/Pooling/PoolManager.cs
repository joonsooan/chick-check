using System;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    [Serializable]
    public class PrewarmConfig
    {
        public GameObject prefab;
        public int count;
        public Transform parent;
    }

    [SerializeField] private PrewarmConfig[] prewarm;

    private readonly Dictionary<GameObject, ObjectPool> pools = new Dictionary<GameObject, ObjectPool>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (prewarm != null)
        {
            foreach (PrewarmConfig config in prewarm)
            {
                if (config.prefab != null)
                {
                    Transform root = config.parent;
                    pools[config.prefab] = new ObjectPool(config.prefab, root, config.count);
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private ObjectPool GetPool(GameObject prefab)
    {
        if (!pools.TryGetValue(prefab, out ObjectPool pool))
        {
            return null;
        }
        return pool;
    }

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        return GetPool(prefab)?.Get(position, rotation, parent);
    }

    public T Spawn<T>(T prefabComponent, Vector3 position, Quaternion rotation, Transform parent = null)
        where T : Component
    {
        return Spawn(prefabComponent.gameObject, position, rotation, parent)?.GetComponent<T>();
    }

    public static void Despawn(GameObject instance)
    {
        PooledObject pooled = instance.GetComponent<PooledObject>();
        if (pooled != null)
        {
            pooled.Release();
        }
        else
        {
            Destroy(instance);
        }
    }
}

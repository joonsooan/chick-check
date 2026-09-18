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
    private readonly Dictionary<GameObject, Transform> configuredParents = new Dictionary<GameObject, Transform>();

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
                    if (config.parent != null)
                    {
                        configuredParents[config.prefab] = config.parent;
                    }
                    GetOrCreatePool(config.prefab, config.count);
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

    private ObjectPool GetOrCreatePool(GameObject prefab, int prewarmCount = 0)
    {
        if (!pools.TryGetValue(prefab, out ObjectPool pool))
        {
            Transform root;
            if (configuredParents.TryGetValue(prefab, out Transform assigned) && assigned != null)
            {
                root = assigned;
            }
            else
            {
                root = new GameObject("Pool_" + prefab.name).transform;
                root.SetParent(transform, false);
            }
            pool = new ObjectPool(prefab, root, prewarmCount);
            pools.Add(prefab, pool);
        }
        return pool;
    }

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        return GetOrCreatePool(prefab).Get(position, rotation, parent);
    }

    public T Spawn<T>(T prefabComponent, Vector3 position, Quaternion rotation, Transform parent = null)
        where T : Component
    {
        return GetOrCreatePool(prefabComponent.gameObject)
            .Get(position, rotation, parent)
            .GetComponent<T>();
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

using System;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    [Serializable]
    public class PrewarmConfig
    {
        public GameObject prefab;
        public int count;
        public Transform parent;
    }

    [SerializeField] private PrewarmConfig[] prewarm;

    private readonly Dictionary<GameObject, ObjectPool> pools = new Dictionary<GameObject, ObjectPool>();

    protected override void Awake()
    {
        base.Awake();

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

    private ObjectPool GetPool(GameObject prefab)
    {
        if (!pools.TryGetValue(prefab, out ObjectPool pool))
        {
            pool = new ObjectPool(prefab, transform, 0);
            pools[prefab] = pool;
            Debug.LogWarning($"PoolManager: auto-created pool for {prefab.name}. Add it to prewarm.", this);
        }

        return pool;
    }

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        return GetPool(prefab).Get(position, rotation, parent).gameObject;
    }

    public T Spawn<T>(T prefabComponent, Vector3 position, Quaternion rotation, Transform parent = null)
        where T : Component
    {
        return GetPool(prefabComponent.gameObject).Get(position, rotation, parent).GetCached<T>();
    }

    public static void Despawn(GameObject instance)
    {
        if (instance.TryGetComponent(out PooledObject pooled))
        {
            pooled.Release();
        }
        else
        {
            Destroy(instance);
        }
    }

    public static void Despawn(PooledObject pooled)
    {
        pooled.Release();
    }
}

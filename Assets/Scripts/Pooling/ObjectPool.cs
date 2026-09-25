using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    private readonly GameObject prefab;
    private readonly Transform root;
    private readonly Queue<PooledObject> inactive = new Queue<PooledObject>();

    public ObjectPool(GameObject prefab, Transform root, int prewarm)
    {
        this.prefab = prefab;
        this.root = root;

        for (int i = 0; i < prewarm; i++)
        {
            PooledObject pooled = CreateInstance();
            pooled.gameObject.SetActive(false);
            inactive.Enqueue(pooled);
        }
    }

    private PooledObject CreateInstance()
    {
        GameObject go = Object.Instantiate(prefab, root);

        PooledObject pooled = go.GetComponent<PooledObject>();
        if (pooled == null)
        {
            pooled = go.AddComponent<PooledObject>();
        }

        pooled.Pool = this;

        return pooled;
    }

    public PooledObject Get(Vector3 position, Quaternion rotation, Transform parent)
    {
        PooledObject pooled = inactive.Count > 0 ? inactive.Dequeue() : CreateInstance();

        Transform t = pooled.transform;
        t.SetParent(parent != null ? parent : root, false);
        t.SetPositionAndRotation(position, rotation);
        pooled.gameObject.SetActive(true);

        return pooled;
    }

    public void Release(PooledObject pooled)
    {
        pooled.gameObject.SetActive(false);
        pooled.transform.SetParent(root, false);
        inactive.Enqueue(pooled);
    }
}

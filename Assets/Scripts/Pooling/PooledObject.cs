using System.Collections.Generic;
using UnityEngine;

public class PooledObject : MonoBehaviour
{
    public ObjectPool Pool { get; set; }

    private IPoolable[] poolables;
    private bool cached;

    private void EnsureCache()
    {
        if (cached)
        {
            return;
        }

        List<IPoolable> found = new List<IPoolable>();
        GetComponentsInChildren(true, found);
        poolables = found.ToArray();
        cached = true;
    }

    public void InvokeSpawn()
    {
        EnsureCache();
        for (int i = 0; i < poolables.Length; i++)
        {
            poolables[i].OnSpawn();
        }
    }

    public void InvokeDespawn()
    {
        EnsureCache();
        for (int i = 0; i < poolables.Length; i++)
        {
            poolables[i].OnDespawn();
        }
    }

    public void Release()
    {
        if (Pool != null)
        {
            Pool.Release(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

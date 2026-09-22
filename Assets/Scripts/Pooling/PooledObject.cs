using System;
using System.Collections.Generic;
using UnityEngine;

public class PooledObject : MonoBehaviour
{
    public ObjectPool Pool { get; set; }

    private readonly Dictionary<Type, Component> componentCache = new Dictionary<Type, Component>();

    public T GetCached<T>() where T : Component
    {
        Type type = typeof(T);
        if (componentCache.TryGetValue(type, out Component component))
        {
            return component as T;
        }

        T found = GetComponent<T>();
        componentCache[type] = found;
        return found;
    }

    public void Release()
    {
        if (Pool != null)
        {
            Pool.Release(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

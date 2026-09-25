using System;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeState
{
    private readonly Dictionary<Type, object> slots = new Dictionary<Type, object>();

    public StatModifierTable Modifiers { get; } = new StatModifierTable();

    public GameObject PendingMirrorPrefab { get; private set; }

    public T Get<T>() where T : class, new()
    {
        if (!slots.TryGetValue(typeof(T), out object slot))
        {
            slot = new T();
            slots[typeof(T)] = slot;
        }

        return (T)slot;
    }

    internal void RequestMirrorUpgrade(GameObject prefab)
    {
        PendingMirrorPrefab = prefab;
    }

    internal GameObject ConsumePendingMirrorPrefab()
    {
        GameObject prefab = PendingMirrorPrefab;
        PendingMirrorPrefab = null;
        return prefab;
    }

    public void Reset()
    {
        Modifiers.Clear();
        slots.Clear();
        PendingMirrorPrefab = null;
    }
}

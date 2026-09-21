using System;
using System.Collections.Generic;
using UnityEngine;

public class SpatialHash<T> where T : class
{
    private readonly float cellSize;
    private readonly Dictionary<Vector2Int, List<T>> buckets = new Dictionary<Vector2Int, List<T>>();
    private readonly Dictionary<T, Vector2Int> cellByItem = new Dictionary<T, Vector2Int>();

    public int Count => cellByItem.Count;

    public SpatialHash(float cellSize)
    {
        if (cellSize <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(cellSize));
        }

        this.cellSize = cellSize;
    }

    public Vector2Int CellOf(Vector3 position)
    {
        return new Vector2Int(
            Mathf.FloorToInt(position.x / cellSize),
            Mathf.FloorToInt(position.y / cellSize));
    }

    public void Insert(T item, Vector3 position)
    {
        if (cellByItem.ContainsKey(item))
        {
            Move(item, position);
            return;
        }

        Vector2Int cell = CellOf(position);
        GetOrCreateBucket(cell).Add(item);
        cellByItem[item] = cell;
    }

    public bool Remove(T item)
    {
        if (!cellByItem.TryGetValue(item, out Vector2Int cell))
        {
            return false;
        }

        if (buckets.TryGetValue(cell, out List<T> bucket))
        {
            bucket.Remove(item);
        }

        cellByItem.Remove(item);
        return true;
    }

    public void Move(T item, Vector3 position)
    {
        if (!cellByItem.TryGetValue(item, out Vector2Int oldCell))
        {
            return;
        }

        Move(item, oldCell, position, out _);
    }

    public bool Move(T item, Vector2Int oldCell, Vector3 position, out Vector2Int newCell)
    {
        newCell = CellOf(position);
        if (newCell == oldCell)
        {
            return false;
        }

        if (buckets.TryGetValue(oldCell, out List<T> oldBucket))
        {
            oldBucket.Remove(item);
        }

        GetOrCreateBucket(newCell).Add(item);
        cellByItem[item] = newCell;
        return true;
    }

    public void Query(Vector3 center, float radius, List<T> results)
    {
        Vector2Int min = CellOf(center - new Vector3(radius, radius, 0f));
        Vector2Int max = CellOf(center + new Vector3(radius, radius, 0f));

        for (int x = min.x; x <= max.x; x++)
        {
            for (int y = min.y; y <= max.y; y++)
            {
                if (buckets.TryGetValue(new Vector2Int(x, y), out List<T> bucket))
                {
                    for (int i = 0; i < bucket.Count; i++)
                    {
                        results.Add(bucket[i]);
                    }
                }
            }
        }
    }

    public void QueryBox(Vector3 center, Vector2 halfExtents, List<T> results)
    {
        Vector2Int min = CellOf(center - new Vector3(halfExtents.x, halfExtents.y, 0f));
        Vector2Int max = CellOf(center + new Vector3(halfExtents.x, halfExtents.y, 0f));

        for (int x = min.x; x <= max.x; x++)
        {
            for (int y = min.y; y <= max.y; y++)
            {
                if (buckets.TryGetValue(new Vector2Int(x, y), out List<T> bucket))
                {
                    for (int i = 0; i < bucket.Count; i++)
                    {
                        results.Add(bucket[i]);
                    }
                }
            }
        }
    }

    private List<T> GetOrCreateBucket(Vector2Int cell)
    {
        if (!buckets.TryGetValue(cell, out List<T> bucket))
        {
            bucket = new List<T>();
            buckets[cell] = bucket;
        }

        return bucket;
    }
}

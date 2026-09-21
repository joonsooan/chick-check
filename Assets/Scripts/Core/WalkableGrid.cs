using System;
using System.Collections.Generic;
using UnityEngine;

public class WalkableGrid
{
    private readonly HashSet<Vector3Int> walkableCells;
    private readonly HashSet<Vector3Int> blocked = new HashSet<Vector3Int>();

    public WalkableGrid(HashSet<Vector3Int> walkableCells)
    {
        this.walkableCells = walkableCells;
    }

    public bool IsWithinBounds(Vector3Int cell)
    {
        return walkableCells == null || walkableCells.Contains(cell);
    }

    public bool IsWalkable(Vector3Int cell)
    {
        return IsWithinBounds(cell) && !blocked.Contains(cell);
    }

    public void SetBlocked(Vector3Int cell, bool value)
    {
        if (value)
        {
            blocked.Add(cell);
        }
        else
        {
            blocked.Remove(cell);
        }
    }

    public bool TryFindWalkable(
        Vector3 fallback,
        int attempts,
        Func<int, Vector3> candidateGenerator,
        Func<Vector3, Vector3Int> worldToCell,
        out Vector3 result)
    {
        for (int attempt = 0; attempt < attempts; attempt++)
        {
            Vector3 candidate = candidateGenerator(attempt);
            if (IsWalkable(worldToCell(candidate)))
            {
                result = candidate;
                return true;
            }
        }

        result = fallback;
        return false;
    }
}

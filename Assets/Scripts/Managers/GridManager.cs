using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : Singleton<GridManager>
{
    [SerializeField] private Grid grid;
    [SerializeField] private Tilemap walkableAreaTilemap;

    private WalkableGrid walkableGrid;

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this)
        {
            return;
        }

        walkableGrid = BuildWalkableGrid();
    }

    private WalkableGrid BuildWalkableGrid()
    {
        if (walkableAreaTilemap == null)
        {
            return new WalkableGrid(null);
        }

        HashSet<Vector3Int> walkable = new HashSet<Vector3Int>();
        foreach (Vector3Int cell in walkableAreaTilemap.cellBounds.allPositionsWithin)
        {
            if (walkableAreaTilemap.HasTile(cell))
            {
                walkable.Add(cell);
            }
        }

        return new WalkableGrid(walkable);
    }

    public Vector3Int WorldToCell(Vector3 worldPos) => grid.WorldToCell(worldPos);

    public bool IsWithinBounds(Vector3 worldPos) => walkableGrid.IsWithinBounds(grid.WorldToCell(worldPos));

    public bool IsWalkable(Vector3 worldPos) => walkableGrid.IsWalkable(grid.WorldToCell(worldPos));

    public void SetBlocked(Vector3Int cell, bool value) => walkableGrid.SetBlocked(cell, value);

    public bool TryFindWalkable(Vector3 fallback, int attempts, Func<int, Vector3> candidateGenerator, out Vector3 result)
    {
        return walkableGrid.TryFindWalkable(fallback, attempts, candidateGenerator, grid.WorldToCell, out result);
    }

    public void ForEachCellInBox(Vector2 worldCenter, Vector2 worldSize, Action<Vector3Int> action)
    {
        Vector3 min = new Vector3(worldCenter.x - worldSize.x * 0.5f, worldCenter.y - worldSize.y * 0.5f);
        Vector3 max = new Vector3(worldCenter.x + worldSize.x * 0.5f, worldCenter.y + worldSize.y * 0.5f);

        Vector3Int minCell = grid.WorldToCell(min);
        Vector3Int maxCell = grid.WorldToCell(max);

        for (int x = minCell.x; x <= maxCell.x; x++)
        {
            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                action(new Vector3Int(x, y, 0));
            }
        }
    }
}

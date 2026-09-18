using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [SerializeField] private Grid grid;

    private readonly HashSet<Vector3Int> blocked = new HashSet<Vector3Int>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public Vector3Int WorldToCell(Vector3 worldPos) => grid.WorldToCell(worldPos);

    public bool IsWalkable(Vector3 worldPos) => !blocked.Contains(grid.WorldToCell(worldPos));

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

    public void ForEachCellInBox(Vector2 worldCenter, Vector2 worldSize, System.Action<Vector3Int> action)
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

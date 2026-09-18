using System.Collections.Generic;
using UnityEngine;

public class GridObstacle : MonoBehaviour
{
    [Header("Footprint")]
    [SerializeField] private Vector2 footprintOffset;
    [SerializeField] private Vector2 footprintSize = Vector2.one;

    private readonly List<Vector3Int> registered = new List<Vector3Int>();

    private void Start()
    {
        Register();
    }

    private void OnDisable()
    {
        Unregister();
    }

    private void Register()
    {
        if (GridManager.Instance == null)
        {
            return;
        }

        Vector2 center = (Vector2)transform.position + footprintOffset;
        GridManager.Instance.ForEachCellInBox(center, footprintSize, cell =>
        {
            GridManager.Instance.SetBlocked(cell, true);
            registered.Add(cell);
        });
    }

    private void Unregister()
    {
        if (GridManager.Instance != null)
        {
            for (int i = 0; i < registered.Count; i++)
            {
                GridManager.Instance.SetBlocked(registered[i], false);
            }
        }
        registered.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.5f);
        Vector3 center = transform.position + (Vector3)footprintOffset;
        Gizmos.DrawWireCube(center, footprintSize);
    }
}

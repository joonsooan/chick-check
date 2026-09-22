using System.Collections.Generic;
using UnityEngine;

public class EntityTickManager : MonoBehaviour
{
    [SerializeField] private int slowTickSlices = 4;

    private static readonly List<PlaceableEntity> slowBuffer = new List<PlaceableEntity>();

    private int slowCursor;

    private void Start()
    {
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnModifiersChanged += HandleModifiersChanged;
        }
    }

    private void OnDestroy()
    {
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnModifiersChanged -= HandleModifiersChanged;
        }
    }

    private void HandleModifiersChanged()
    {
        var entities = PlaceableEntity.ActiveEntities;
        for (int i = 0; i < entities.Count; i++)
        {
            entities[i].RefreshStats();
        }
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f)
        {
            return;
        }

        var entities = PlaceableEntity.ActiveEntities;
        for (int i = entities.Count - 1; i >= 0; i--)
        {
            entities[i].Tick(dt);
        }

        RunSlowTick(entities);
    }

    private void RunSlowTick(IReadOnlyList<PlaceableEntity> entities)
    {
        int count = entities.Count;
        if (count == 0)
        {
            slowCursor = 0;
            return;
        }

        int slices = Mathf.Max(1, slowTickSlices);
        int perFrame = Mathf.CeilToInt(count / (float)slices);

        slowBuffer.Clear();
        for (int n = 0; n < perFrame; n++)
        {
            if (slowCursor >= count)
            {
                slowCursor = 0;
            }

            slowBuffer.Add(entities[slowCursor]);
            slowCursor++;
        }

        float now = Time.time;
        for (int i = 0; i < slowBuffer.Count; i++)
        {
            PlaceableEntity entity = slowBuffer[i];
            if (entity.IsActiveEntity)
            {
                entity.SlowTick(now);
            }
        }
    }
}

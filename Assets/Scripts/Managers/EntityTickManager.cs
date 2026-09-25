using System.Collections.Generic;
using UnityEngine;

public class EntityTickManager : MonoBehaviour
{
    [SerializeField] private int slowTickSlices = 4;

    private static readonly List<PlaceableEntity> slowTickBatch = new List<PlaceableEntity>();

    private int slowTickCursor;

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
        IReadOnlyList<PlaceableEntity> entities = PlaceableEntity.ActiveEntities;
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

        IReadOnlyList<PlaceableEntity> entities = PlaceableEntity.ActiveEntities;
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
            slowTickCursor = 0;
            return;
        }

        int slices = Mathf.Max(1, slowTickSlices);
        int perFrame = Mathf.CeilToInt(count / (float)slices);

        slowTickBatch.Clear();
        for (int n = 0; n < perFrame; n++)
        {
            if (slowTickCursor >= count)
            {
                slowTickCursor = 0;
            }

            slowTickBatch.Add(entities[slowTickCursor]);
            slowTickCursor++;
        }

        float time = Time.time;
        for (int i = 0; i < slowTickBatch.Count; i++)
        {
            PlaceableEntity entity = slowTickBatch[i];
            if (entity.IsRegistered)
            {
                entity.SlowTick(time);
            }
        }
    }
}

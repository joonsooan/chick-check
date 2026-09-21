using UnityEngine;

public class EntityTickManager : MonoBehaviour
{
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
    }
}

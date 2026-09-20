using UnityEngine;

public class EntityTickManager : MonoBehaviour
{
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

using System.Collections;
using UnityEngine;

public abstract class PlaceableEntity : MonoBehaviour
{
    protected SpriteRenderer spriteRenderer;

    public SpriteRenderer SpriteRenderer => spriteRenderer;

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected IEnumerator GrowthRoutine(EconomyEntityData data)
    {
        if (!data.CanGrow)
        {
            yield break;
        }

        while (true)
        {
            yield return new WaitForSeconds(data.GrowthInterval);

            if (Random.value <= data.GrowthChance)
            {
                GrowToNextStage(data.NextStagePrefab);
                yield break;
            }
        }
    }

    private void GrowToNextStage(GameObject nextStagePrefab)
    {
        GameObject next = PoolManager.Instance.Spawn(nextStagePrefab, transform.position, transform.rotation, transform.parent);
        next.GetComponent<PlaceableEntity>()?.NotifySpawnCount();
        NotifyDespawnCount();
        PoolManager.Despawn(gameObject);
    }

    protected virtual void NotifySpawnCount()
    {
    }

    protected virtual void NotifyDespawnCount()
    {
    }
}

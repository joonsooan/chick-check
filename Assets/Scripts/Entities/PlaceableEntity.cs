using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlaceableEntity : MonoBehaviour
{
    private static readonly List<PlaceableEntity> activeEntities = new List<PlaceableEntity>();

    public static IReadOnlyList<PlaceableEntity> ActiveEntities => activeEntities;

    protected SpriteRenderer spriteRenderer;

    public SpriteRenderer SpriteRenderer => spriteRenderer;
    public bool IsCarried { get; private set; }

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected virtual void OnEnable()
    {
        activeEntities.Add(this);
    }

    protected virtual void OnDisable()
    {
        activeEntities.Remove(this);
    }

    public virtual void SetCarried(bool carried)
    {
        IsCarried = carried;
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

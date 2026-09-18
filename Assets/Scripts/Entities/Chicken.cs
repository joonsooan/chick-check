using UnityEngine;

public class Chicken : PlaceableEntity
{
    [SerializeField] private ChickenData data;

    protected override void Awake()
    {
        base.Awake();

        spriteRenderer.sprite = data.Sprite;
    }

    private void OnEnable()
    {
        StartCoroutine(GrowthRoutine(data));
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    protected override void NotifySpawnCount()
    {
        SpawnManager.Instance?.RegisterChickenSpawned();
    }

    protected override void NotifyDespawnCount()
    {
        SpawnManager.Instance?.RegisterChickenDespawned();
    }
}

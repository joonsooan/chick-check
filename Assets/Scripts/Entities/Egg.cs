using UnityEngine;

public class Egg : PlaceableEntity
{
    [SerializeField] private EggData data;

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
        SpawnManager.Instance?.RegisterEggSpawned();
    }

    protected override void NotifyDespawnCount()
    {
        SpawnManager.Instance?.RegisterEggDespawned();
    }
}

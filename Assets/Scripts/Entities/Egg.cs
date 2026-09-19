using UnityEngine;

public class Egg : PlaceableEntity
{
    [SerializeField] private EggData data;

    public override EconomyEntityData EconomyData => data;

    protected override void Awake()
    {
        base.Awake();

        spriteRenderer.sprite = data.Sprite;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        StartCoroutine(GrowthRoutine(data));
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        StopAllCoroutines();
    }

    public override void NotifySpawnCount()
    {
        SpawnManager.Instance?.RegisterEggSpawned();
    }

    public override void NotifyDespawnCount()
    {
        SpawnManager.Instance?.RegisterEggDespawned();
    }
}

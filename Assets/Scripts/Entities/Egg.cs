using UnityEngine;

public class Egg : PlaceableEntity
{
    [SerializeField] private EggData data;

    public override EconomyEntityData EconomyData => data;

    protected override ShopStatType GrowthChanceStat => ShopStatType.EggGrowthChance;

    public override ShopStatType SellPriceStat => ShopStatType.SellPriceEgg;

    protected override void OnEnable()
    {
        base.OnEnable();

        spriteRenderer.sprite = data.Sprite;
        StartCoroutine(GrowthRoutine(data));
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        StopAllCoroutines();
    }

    public override void SetCarried(bool carried)
    {
        base.SetCarried(carried);

        StopAllCoroutines();

        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        if (!carried)
        {
            StartCoroutine(GrowthRoutine(data));
        }
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

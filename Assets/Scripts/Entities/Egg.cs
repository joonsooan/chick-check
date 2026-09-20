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

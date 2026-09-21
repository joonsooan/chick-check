using UnityEngine;

public class Egg : PlaceableEntity
{
    [SerializeField] private EggData data;

    public override EconomyEntityData EconomyData => data;

    protected override ShopStatType GrowthChanceStat => ShopStatType.EggGrowthChance;

    public override ShopStatType SellPriceStat => ShopStatType.SellPriceEgg;
}

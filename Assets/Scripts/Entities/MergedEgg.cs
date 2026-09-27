public class MergedEgg : Egg
{
    public override ShopStatType SellPriceStat => ShopStatType.SellPriceMergedEgg;

    protected override ShopStatType GrowthChanceStat => ShopStatType.MergedEggGrowthChance;
}

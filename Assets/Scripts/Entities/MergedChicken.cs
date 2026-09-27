public class MergedChicken : Chicken
{
    public override ShopStatType SellPriceStat => ShopStatType.SellPriceMergedChicken;

    protected override ShopStatType LayChanceStat => ShopStatType.MergedEggLayChance;
}

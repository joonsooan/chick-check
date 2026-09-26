public class MergedChick : Chick
{
    public override ShopStatType SellPriceStat => ShopStatType.SellPriceMergedChick;

    protected override ShopStatType GrowthChanceStat => ShopStatType.MergedChickGrowthChance;

    protected override bool CanMutate => false;

    protected override bool CanGrowNow() => true;
}

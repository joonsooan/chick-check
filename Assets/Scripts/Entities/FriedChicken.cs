using UnityEngine;

public class FriedChicken : PlaceableEntity
{
    [SerializeField] private FriedChickenData data;

    public override EconomyEntityData EconomyData => data;

    public override ShopStatType SellPriceStat => ShopStatType.SellPriceFriedChicken;
}

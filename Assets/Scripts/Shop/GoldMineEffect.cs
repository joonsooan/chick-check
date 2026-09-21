using System;
using UnityEngine;

[Serializable]
public class GoldMineEffect : ShopEffect
{
    [SerializeField] private float interval = 5f;
    [SerializeField] private float chance = 0.5f;
    [SerializeField] private int baseGold = 1;
    [SerializeField] private int goldPerPurchase = 1;

    public float Interval => interval;
    public float Chance => chance;
    public int BaseGold => baseGold;
    public int GoldPerPurchase => goldPerPurchase;

    public override void Apply(UpgradeState state)
    {
        state.GoldMineInterval = interval;
        state.GoldMineChance = chance;
        state.GoldMineBaseGold = baseGold;
        state.GoldMineGoldPerPurchase = goldPerPurchase;
        state.GoldMineLevel++;
    }
}

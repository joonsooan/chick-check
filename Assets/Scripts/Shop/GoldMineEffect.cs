using System;
using UnityEngine;

[Serializable]
public class GoldMineEffect : ShopEffect
{
    [SerializeField] private float interval = 5f;
    [SerializeField] private float chance = 0.5f;
    [SerializeField] private int baseGold = 1;
    [SerializeField] private int goldPerPurchase = 1;

    public override void Apply(UpgradeState state)
    {
        GoldMineState goldMine = state.Get<GoldMineState>();
        goldMine.Interval = interval;
        goldMine.Chance = chance;
        goldMine.BaseGold = baseGold;
        goldMine.GoldPerExtraLevel = goldPerPurchase;
        goldMine.Level++;
    }
}

public class GoldMineState
{
    public int Level { get; internal set; }
    public float Interval { get; internal set; }
    public float Chance { get; internal set; }
    public int BaseGold { get; internal set; }
    public int GoldPerExtraLevel { get; internal set; }
    public int GoldAmount => Level <= 0 ? 0 : BaseGold + GoldPerExtraLevel * (Level - 1);
}

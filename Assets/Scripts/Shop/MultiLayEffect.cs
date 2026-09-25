using System;
using UnityEngine;

[Serializable]
public class MultiLayEffect : ShopEffect
{
    [SerializeField] private float chance = 0.5f;
    [SerializeField] private int countPerPurchase = 1;

    public override void Apply(UpgradeState state)
    {
        MultiLayState multiLay = state.Get<MultiLayState>();
        multiLay.Chance = chance;
        multiLay.BonusEggCount += countPerPurchase;
    }
}

public class MultiLayState
{
    public float Chance { get; internal set; }
    public int BonusEggCount { get; internal set; }
}

using System;
using UnityEngine;

[Serializable]
public class MultiLayEffect : ShopEffect
{
    [SerializeField] private float chance = 0.5f;
    [SerializeField] private int countPerPurchase = 1;

    public float Chance => chance;
    public int CountPerPurchase => countPerPurchase;

    public override void Apply(UpgradeState state)
    {
        state.MultiLayChance = chance;
        state.MultiLayBonusCount += countPerPurchase;
    }
}

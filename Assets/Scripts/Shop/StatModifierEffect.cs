using System;
using UnityEngine;

[Serializable]
public class StatModifierEffect : ShopEffect
{
    [SerializeField] private ShopStatType targetStat;
    [SerializeField] private ShopModifierOp op;
    [SerializeField] private float value = 1f;

    public override void Apply(UpgradeState state)
    {
        if (op == ShopModifierOp.Add)
        {
            state.Modifiers.AddAdditive(targetStat, value);
        }
        else
        {
            state.Modifiers.AddMultiplier(targetStat, value);
        }
    }
}

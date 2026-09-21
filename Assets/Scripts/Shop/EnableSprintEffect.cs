using System;

[Serializable]
public class EnableSprintEffect : ShopEffect
{
    public override void Apply(UpgradeState state)
    {
        state.SprintEnabled = true;
    }
}

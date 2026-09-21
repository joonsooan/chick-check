using System;

[Serializable]
public class EnableContagionEffect : ShopEffect
{
    public override void Apply(UpgradeState state)
    {
        state.ContagionEnabled = true;
    }

    public override bool IsAvailable(UpgradeState state)
    {
        return state.MutantSpawnCount > 0;
    }
}

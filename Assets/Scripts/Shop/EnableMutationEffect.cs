using System;

[Serializable]
public class EnableMutationEffect : ShopEffect
{
    public override void Apply(UpgradeState state)
    {
        state.MutantSpawnCount++;
    }
}

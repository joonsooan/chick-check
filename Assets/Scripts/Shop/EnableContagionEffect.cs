using System;

[Serializable]
public class EnableContagionEffect : ShopEffect
{
    public override void Apply(UpgradeState state)
    {
        state.Get<ContagionState>().Enabled = true;
    }

    public override bool IsAvailable(UpgradeState state)
    {
        return state.Get<MutationState>().MutantSpawnCountPerRound > 0;
    }
}

public class ContagionState
{
    public bool Enabled { get; internal set; }
}

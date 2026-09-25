using System;

[Serializable]
public class EnableMutationEffect : ShopEffect
{
    public override void Apply(UpgradeState state)
    {
        state.Get<MutationState>().MutantSpawnCountPerRound++;
    }
}

public class MutationState
{
    public int MutantSpawnCountPerRound { get; internal set; }
}

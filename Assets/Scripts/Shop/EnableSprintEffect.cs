using System;

[Serializable]
public class EnableSprintEffect : ShopEffect
{
    public override void Apply(UpgradeState state)
    {
        state.Get<SprintState>().Enabled = true;
    }
}

public class SprintState
{
    public bool Enabled { get; internal set; }
}

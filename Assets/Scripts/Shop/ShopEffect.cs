using System;

[Serializable]
public abstract class ShopEffect
{
    public abstract void Apply(UpgradeState state);

    public virtual bool IsAvailable(UpgradeState state)
    {
        return true;
    }
}

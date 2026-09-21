using System.Collections.Generic;

public class StatModifierTable
{
    private readonly Dictionary<ShopStatType, float> additiveBonus = new Dictionary<ShopStatType, float>();
    private readonly Dictionary<ShopStatType, float> multiplier = new Dictionary<ShopStatType, float>();

    public void AddAdditive(ShopStatType stat, float value)
    {
        additiveBonus.TryGetValue(stat, out float current);
        additiveBonus[stat] = current + value;
    }

    public void AddMultiplier(ShopStatType stat, float value)
    {
        multiplier.TryGetValue(stat, out float current);
        multiplier[stat] = (current == 0f ? 1f : current) + (value - 1f);
    }

    public float GetAdditive(ShopStatType stat)
    {
        return additiveBonus.TryGetValue(stat, out float value) ? value : 0f;
    }

    public float GetMultiplier(ShopStatType stat)
    {
        return multiplier.TryGetValue(stat, out float value) ? value : 1f;
    }

    public float Apply(ShopStatType stat, float baseValue)
    {
        return (baseValue + GetAdditive(stat)) * GetMultiplier(stat);
    }

    public void Clear()
    {
        additiveBonus.Clear();
        multiplier.Clear();
    }
}

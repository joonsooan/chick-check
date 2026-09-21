using NUnit.Framework;

[TestFixture]
public class StatModifierTableTests
{
    [Test]
    public void AddAdditive_AccumulatesValues()
    {
        StatModifierTable table = new StatModifierTable();
        table.AddAdditive(ShopStatType.GrowthChance, 0.05f);
        table.AddAdditive(ShopStatType.GrowthChance, 0.05f);

        Assert.AreEqual(0.1f, table.GetAdditive(ShopStatType.GrowthChance), 0.0001f);
    }

    [Test]
    public void AddMultiplier_AccumulatesWithCurrentRule()
    {
        StatModifierTable table = new StatModifierTable();
        table.AddMultiplier(ShopStatType.GrowthChance, 1.1f);
        table.AddMultiplier(ShopStatType.GrowthChance, 1.1f);

        Assert.AreEqual(1.2f, table.GetMultiplier(ShopStatType.GrowthChance), 0.0001f);
    }

    [Test]
    public void Apply_AddsThenMultiplies()
    {
        StatModifierTable table = new StatModifierTable();
        table.AddAdditive(ShopStatType.GrowthChance, 2f);
        table.AddMultiplier(ShopStatType.GrowthChance, 1.5f);

        Assert.AreEqual(18f, table.Apply(ShopStatType.GrowthChance, 10f), 0.0001f);
    }

    [Test]
    public void Apply_UnregisteredStat_ReturnsBaseValue()
    {
        StatModifierTable table = new StatModifierTable();

        Assert.AreEqual(10f, table.Apply(ShopStatType.SellPriceChick, 10f), 0.0001f);
    }
}

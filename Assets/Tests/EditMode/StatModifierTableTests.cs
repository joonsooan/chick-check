using NUnit.Framework;

[TestFixture]
public class StatModifierTableTests
{
    [Test]
    public void AddAdditive_AccumulatesValues()
    {
        StatModifierTable table = new StatModifierTable();
        table.AddAdditive(ShopStatType.ChickGrowthChance, 0.05f);
        table.AddAdditive(ShopStatType.ChickGrowthChance, 0.05f);

        Assert.AreEqual(0.1f, table.GetAdditive(ShopStatType.ChickGrowthChance), 0.0001f);
    }

    [Test]
    public void AddMultiplier_AccumulatesWithCurrentRule()
    {
        StatModifierTable table = new StatModifierTable();
        table.AddMultiplier(ShopStatType.ChickGrowthChance, 1.1f);
        table.AddMultiplier(ShopStatType.ChickGrowthChance, 1.1f);

        Assert.AreEqual(1.2f, table.GetMultiplier(ShopStatType.ChickGrowthChance), 0.0001f);
    }

    [Test]
    public void Apply_AddsThenMultiplies()
    {
        StatModifierTable table = new StatModifierTable();
        table.AddAdditive(ShopStatType.ChickGrowthChance, 2f);
        table.AddMultiplier(ShopStatType.ChickGrowthChance, 1.5f);

        Assert.AreEqual(18f, table.Apply(ShopStatType.ChickGrowthChance, 10f), 0.0001f);
    }

    [Test]
    public void AddMultiplier_AccumulatedZeroIsNotTreatedAsUnset()
    {
        StatModifierTable table = new StatModifierTable();
        table.AddMultiplier(ShopStatType.ChickGrowthChance, 0.5f);
        table.AddMultiplier(ShopStatType.ChickGrowthChance, 0.5f);
        table.AddMultiplier(ShopStatType.ChickGrowthChance, 1.1f);

        Assert.AreEqual(0.1f, table.GetMultiplier(ShopStatType.ChickGrowthChance), 0.0001f);
    }

    [Test]
    public void Apply_UnregisteredStat_ReturnsBaseValue()
    {
        StatModifierTable table = new StatModifierTable();

        Assert.AreEqual(10f, table.Apply(ShopStatType.SellPriceChick, 10f), 0.0001f);
    }
}

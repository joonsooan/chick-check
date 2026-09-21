using System.Collections.Generic;
using NUnit.Framework;

[TestFixture]
public class RoundSettlementTests
{
    [Test]
    public void Settle_AppliesModifierAndRounds()
    {
        StatModifierTable modifiers = new StatModifierTable();
        modifiers.AddMultiplier(ShopStatType.SellPriceChick, 1.1f);

        List<SaleInput> inputs = new List<SaleInput>
        {
            new SaleInput(null, 7, 0, ShopStatType.SellPriceChick),
        };

        List<SaleRecord> records = RoundSettlement.Settle(inputs, modifiers.Apply, out int totalGold);

        Assert.AreEqual(1, records.Count);
        Assert.AreEqual(8, records[0].SellPrice);
        Assert.AreEqual(8, totalGold);
    }

    [Test]
    public void Settle_PreservesDisplayOrderAndSprite()
    {
        StatModifierTable modifiers = new StatModifierTable();
        List<SaleInput> inputs = new List<SaleInput>
        {
            new SaleInput(null, 5, 3, ShopStatType.SellPriceChicken),
        };

        List<SaleRecord> records = RoundSettlement.Settle(inputs, modifiers.Apply, out _);

        Assert.AreEqual(3, records[0].DisplayOrder);
        Assert.IsNull(records[0].Sprite);
    }

    [Test]
    public void Settle_SumsTotalGoldAcrossEntries()
    {
        StatModifierTable modifiers = new StatModifierTable();
        List<SaleInput> inputs = new List<SaleInput>
        {
            new SaleInput(null, 5, 0, ShopStatType.SellPriceChick),
            new SaleInput(null, 3, 1, ShopStatType.SellPriceEgg),
        };

        RoundSettlement.Settle(inputs, modifiers.Apply, out int totalGold);

        Assert.AreEqual(8, totalGold);
    }
}

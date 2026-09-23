using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

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

    private static Sprite CreateSprite()
    {
        return Sprite.Create(new Texture2D(1, 1), new Rect(0f, 0f, 1f, 1f), Vector2.zero);
    }

    [Test]
    public void Summarize_GroupsRecordsBySpriteAndSumsPriceAndCount()
    {
        Sprite spriteA = CreateSprite();
        Sprite spriteB = CreateSprite();

        List<SaleRecord> records = new List<SaleRecord>
        {
            new SaleRecord(spriteA, 5, 0),
            new SaleRecord(spriteA, 3, 0),
            new SaleRecord(spriteB, 10, 1),
        };

        List<SaleGroup> groups = RoundSettlement.Summarize(records, out int totalGold);

        Assert.AreEqual(2, groups.Count);
        Assert.AreEqual(8, groups[0].TotalPrice);
        Assert.AreEqual(2, groups[0].Count);
        Assert.AreEqual(10, groups[1].TotalPrice);
        Assert.AreEqual(1, groups[1].Count);
        Assert.AreEqual(18, totalGold);
    }

    [Test]
    public void Summarize_StableSortsByDisplayOrder_PreservesFirstAppearanceOnTies()
    {
        Sprite spriteFirst = CreateSprite();

        List<SaleRecord> records = new List<SaleRecord>
        {
            new SaleRecord(spriteFirst, 1, 5),
            new SaleRecord(spriteFirst, 1, 5),
        };

        List<SaleGroup> groups = RoundSettlement.Summarize(records, out _);

        Assert.AreEqual(1, groups.Count);
        Assert.AreEqual(5, groups[0].DisplayOrder);
    }

    [Test]
    public void Summarize_EmptyInput_ReturnsEmptyGroupsAndZeroGold()
    {
        List<SaleRecord> records = new List<SaleRecord>();

        List<SaleGroup> groups = RoundSettlement.Summarize(records, out int totalGold);

        Assert.AreEqual(0, groups.Count);
        Assert.AreEqual(0, totalGold);
    }

    [Test]
    public void Summarize_PropagatesShowCountFromFirstRecordOfSprite()
    {
        Sprite spriteA = CreateSprite();
        Sprite spriteB = CreateSprite();

        List<SaleRecord> records = new List<SaleRecord>
        {
            new SaleRecord(spriteA, 5, 0),
            new SaleRecord(spriteB, 10, 1, showCount: false),
        };

        List<SaleGroup> groups = RoundSettlement.Summarize(records, out _);

        Assert.IsTrue(groups[0].ShowCount);
        Assert.IsFalse(groups[1].ShowCount);
    }

    [Test]
    public void AppendMinedGold_AddsRecordSortedAfterEntitiesByDisplayOrder()
    {
        Sprite entitySprite = CreateSprite();
        Sprite goldIcon = CreateSprite();

        List<SaleRecord> records = new List<SaleRecord>
        {
            new SaleRecord(entitySprite, 5, 1),
        };

        RoundSettlement.AppendMinedGold(records, goldIcon, 25, 100);

        List<SaleGroup> groups = RoundSettlement.Summarize(records, out int totalGold);

        Assert.AreEqual(2, groups.Count);
        Assert.AreEqual(goldIcon, groups[1].Sprite);
        Assert.AreEqual(25, groups[1].TotalPrice);
        Assert.IsFalse(groups[1].ShowCount);
        Assert.AreEqual(30, totalGold);
    }

    [Test]
    public void AppendMinedGold_NoOpsWhenAmountIsZeroOrIconIsNull()
    {
        Sprite goldIcon = CreateSprite();
        List<SaleRecord> records = new List<SaleRecord>();

        RoundSettlement.AppendMinedGold(records, goldIcon, 0, 100);
        RoundSettlement.AppendMinedGold(records, null, 25, 100);

        Assert.AreEqual(0, records.Count);
    }
}

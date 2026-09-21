using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class ShopCatalogPickerTests
{
    private List<ShopItemData> items;

    [SetUp]
    public void SetUp()
    {
        items = new List<ShopItemData>
        {
            ShopItemData.CreateForTest(itemName: "A"),
            ShopItemData.CreateForTest(itemName: "B"),
            ShopItemData.CreateForTest(itemName: "C"),
            ShopItemData.CreateForTest(itemName: "D", effectKind: ShopEffectKind.EnableContagion),
        };
    }

    [TearDown]
    public void TearDown()
    {
        foreach (ShopItemData item in items)
        {
            Object.DestroyImmediate(item);
        }
    }

    [Test]
    public void Pick_ReturnsRequestedCountWithoutDuplicates()
    {
        List<ShopItemData> result = ShopCatalogPicker.Pick(items, 3, _ => true, new System.Random(1));

        Assert.AreEqual(3, result.Count);
        Assert.AreEqual(3, new HashSet<ShopItemData>(result).Count);
    }

    [Test]
    public void Pick_ExcludesFilteredItems()
    {
        List<ShopItemData> result = ShopCatalogPicker.Pick(
            items, 4, candidate => candidate.EffectKind != ShopEffectKind.EnableContagion, new System.Random(1));

        Assert.AreEqual(3, result.Count);
        Assert.IsFalse(result.Contains(items[3]));
    }

    [Test]
    public void Pick_CountExceedsPool_ReturnsPoolSize()
    {
        List<ShopItemData> result = ShopCatalogPicker.Pick(items, 10, _ => true, new System.Random(1));

        Assert.AreEqual(items.Count, result.Count);
    }
}

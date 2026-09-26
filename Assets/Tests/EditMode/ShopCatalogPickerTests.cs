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
            ShopItemData.CreateForTest(itemName: "D", effect: new EnableContagionEffect()),
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
            items, 4, candidate => !(candidate.Effect is EnableContagionEffect), new System.Random(1));

        Assert.AreEqual(3, result.Count);
        Assert.IsFalse(result.Contains(items[3]));
    }

    [Test]
    public void Pick_CountExceedsPool_ReturnsPoolSize()
    {
        List<ShopItemData> result = ShopCatalogPicker.Pick(items, 10, _ => true, new System.Random(1));

        Assert.AreEqual(items.Count, result.Count);
    }

    [Test]
    public void Pick_WithPinned_IncludesPinnedItemsFirst()
    {
        List<ShopItemData> pinned = new List<ShopItemData> { items[2] };

        List<ShopItemData> result = ShopCatalogPicker.Pick(items, 3, _ => true, new System.Random(1), pinned);

        Assert.AreEqual(3, result.Count);
        Assert.Contains(items[2], result);
    }

    [Test]
    public void Pick_WithPinned_ExcludesUnavailablePinnedItems()
    {
        List<ShopItemData> pinned = new List<ShopItemData> { items[3] };

        List<ShopItemData> result = ShopCatalogPicker.Pick(
            items, 3, candidate => !(candidate.Effect is EnableContagionEffect), new System.Random(1), pinned);

        Assert.AreEqual(3, result.Count);
        Assert.IsFalse(result.Contains(items[3]));
    }

    [Test]
    public void Pick_WithPinned_NoDuplicatesBetweenPinnedAndRandom()
    {
        List<ShopItemData> pinned = new List<ShopItemData> { items[0], items[1] };

        List<ShopItemData> result = ShopCatalogPicker.Pick(items, 4, _ => true, new System.Random(1), pinned);

        Assert.AreEqual(4, new HashSet<ShopItemData>(result).Count);
    }

    [Test]
    public void Pick_WithPinnedExceedingCount_TruncatesToCount()
    {
        List<ShopItemData> pinned = new List<ShopItemData> { items[0], items[1], items[2], items[3] };

        List<ShopItemData> result = ShopCatalogPicker.Pick(items, 2, _ => true, new System.Random(1), pinned);

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(items[0], result[0]);
        Assert.AreEqual(items[1], result[1]);
    }
}

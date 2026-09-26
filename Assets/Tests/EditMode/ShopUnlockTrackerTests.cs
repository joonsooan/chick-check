using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class ShopUnlockTrackerTests
{
    private ShopItemData pinnedItem;
    private ShopItemData unpinnedItem;
    private List<ShopItemData> items;

    [SetUp]
    public void SetUp()
    {
        pinnedItem = ShopItemData.CreateForTest(itemName: "Pinned", pinWhenUnlocked: true);
        unpinnedItem = ShopItemData.CreateForTest(itemName: "Unpinned", pinWhenUnlocked: false);
        items = new List<ShopItemData> { pinnedItem, unpinnedItem };
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
    public void CollectNewlyUnlocked_ReturnsPinnedItemOnFirstAvailability()
    {
        ShopUnlockTracker tracker = new ShopUnlockTracker();

        List<ShopItemData> result = tracker.CollectNewlyUnlocked(items, _ => true);

        Assert.Contains(pinnedItem, result);
    }

    [Test]
    public void CollectNewlyUnlocked_DoesNotReturnAgainOnSecondCall()
    {
        ShopUnlockTracker tracker = new ShopUnlockTracker();
        tracker.CollectNewlyUnlocked(items, _ => true);

        List<ShopItemData> result = tracker.CollectNewlyUnlocked(items, _ => true);

        Assert.IsFalse(result.Contains(pinnedItem));
    }

    [Test]
    public void CollectNewlyUnlocked_IgnoresItemsWithoutPinFlag()
    {
        ShopUnlockTracker tracker = new ShopUnlockTracker();

        List<ShopItemData> result = tracker.CollectNewlyUnlocked(items, _ => true);

        Assert.IsFalse(result.Contains(unpinnedItem));
    }

    [Test]
    public void Clear_AllowsPinnedItemToBeReturnedAgain()
    {
        ShopUnlockTracker tracker = new ShopUnlockTracker();
        tracker.CollectNewlyUnlocked(items, _ => true);

        tracker.Clear();
        List<ShopItemData> result = tracker.CollectNewlyUnlocked(items, _ => true);

        Assert.Contains(pinnedItem, result);
    }
}

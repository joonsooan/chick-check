using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class ShopUnlockTrackerTests
{
    private ShopItemData pinnedItem;
    private ShopItemData oneTimePinnedItem;
    private ShopItemData unpinnedItem;
    private List<ShopItemData> items;

    [SetUp]
    public void SetUp()
    {
        pinnedItem = ShopItemData.CreateForTest(itemName: "Pinned", pinWhenUnlocked: true);
        oneTimePinnedItem = ShopItemData.CreateForTest(itemName: "OneTimePinned", pinWhenUnlocked: true, oneTimeOnly: true);
        unpinnedItem = ShopItemData.CreateForTest(itemName: "Unpinned", pinWhenUnlocked: false);
        items = new List<ShopItemData> { pinnedItem, oneTimePinnedItem, unpinnedItem };
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
    public void BeginShopPhase_PinsItemOnFirstAvailability()
    {
        ShopUnlockTracker tracker = new ShopUnlockTracker();

        tracker.BeginShopPhase(items, _ => true);

        Assert.IsTrue(tracker.Pinned.Contains(pinnedItem));
    }

    [Test]
    public void BeginShopPhase_DropsRepeatableItemOnSecondPhase()
    {
        ShopUnlockTracker tracker = new ShopUnlockTracker();
        tracker.BeginShopPhase(items, _ => true);

        tracker.BeginShopPhase(items, _ => true);

        Assert.IsFalse(tracker.Pinned.Contains(pinnedItem));
    }

    [Test]
    public void BeginShopPhase_KeepsOneTimeItemPinnedUntilUnavailable()
    {
        ShopUnlockTracker tracker = new ShopUnlockTracker();
        tracker.BeginShopPhase(items, _ => true);

        tracker.BeginShopPhase(items, _ => true);
        Assert.IsTrue(tracker.Pinned.Contains(oneTimePinnedItem));

        tracker.BeginShopPhase(items, item => item != oneTimePinnedItem);
        Assert.IsFalse(tracker.Pinned.Contains(oneTimePinnedItem));
    }

    [Test]
    public void BeginShopPhase_IgnoresItemsWithoutPinFlag()
    {
        ShopUnlockTracker tracker = new ShopUnlockTracker();

        tracker.BeginShopPhase(items, _ => true);

        Assert.IsFalse(tracker.Pinned.Contains(unpinnedItem));
    }

    [Test]
    public void RefreshPinned_KeepsAvailableItemsForReroll()
    {
        ShopUnlockTracker tracker = new ShopUnlockTracker();
        tracker.BeginShopPhase(items, _ => true);

        tracker.RefreshPinned(_ => true);

        Assert.IsTrue(tracker.Pinned.Contains(pinnedItem));
        Assert.IsTrue(tracker.Pinned.Contains(oneTimePinnedItem));
    }

    [Test]
    public void RefreshPinned_DropsPurchasedItem()
    {
        ShopUnlockTracker tracker = new ShopUnlockTracker();
        tracker.BeginShopPhase(items, _ => true);

        tracker.RefreshPinned(item => item != oneTimePinnedItem);

        Assert.IsFalse(tracker.Pinned.Contains(oneTimePinnedItem));
    }

    [Test]
    public void Clear_AllowsItemToBePinnedAgain()
    {
        ShopUnlockTracker tracker = new ShopUnlockTracker();
        tracker.BeginShopPhase(items, _ => true);

        tracker.Clear();
        tracker.BeginShopPhase(items, _ => true);

        Assert.IsTrue(tracker.Pinned.Contains(pinnedItem));
    }
}

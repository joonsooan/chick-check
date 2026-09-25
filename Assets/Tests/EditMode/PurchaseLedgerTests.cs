using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class PurchaseLedgerTests
{
    private ShopItemData item;

    [SetUp]
    public void SetUp()
    {
        item = ShopItemData.CreateForTest(cost: 10, costGrowthPerPurchase: 2f);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(item);
    }

    [Test]
    public void GetCurrentCost_ScalesByPurchaseCount()
    {
        PurchaseLedger ledger = new PurchaseLedger();

        Assert.AreEqual(10, ledger.GetCurrentCost(item));

        ledger.RecordPurchase(item);
        Assert.AreEqual(20, ledger.GetCurrentCost(item));

        ledger.RecordPurchase(item);
        Assert.AreEqual(40, ledger.GetCurrentCost(item));
    }

    [Test]
    public void RecordPurchase_OneTimeOnly_Retires()
    {
        ShopItemData oneTime = ShopItemData.CreateForTest(cost: 10, costGrowthPerPurchase: 2f, oneTimeOnly: true);
        PurchaseLedger ledger = new PurchaseLedger();

        Assert.IsFalse(ledger.IsSoldOut(oneTime));
        ledger.RecordPurchase(oneTime);
        Assert.IsTrue(ledger.IsSoldOut(oneTime));

        Object.DestroyImmediate(oneTime);
    }

    [Test]
    public void Clear_ResetsCountsAndRetirement()
    {
        PurchaseLedger ledger = new PurchaseLedger();
        ledger.RecordPurchase(item);

        ledger.Clear();

        Assert.AreEqual(10, ledger.GetCurrentCost(item));
        Assert.IsFalse(ledger.IsSoldOut(item));
    }
}

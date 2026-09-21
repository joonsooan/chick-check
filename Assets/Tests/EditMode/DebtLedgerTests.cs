using NUnit.Framework;

[TestFixture]
public class DebtLedgerTests
{
    [Test]
    public void Constructor_InitializesFromConfig()
    {
        DebtLedger ledger = new DebtLedger(3, 30, 2.2f);

        Assert.AreEqual(30, ledger.RemainingDebt);
        Assert.AreEqual(3, ledger.NextDebtDueRound);
    }

    [Test]
    public void Pay_PartialPayment_ReducesRemaining()
    {
        DebtLedger ledger = new DebtLedger(3, 30, 2.2f);

        int paid = ledger.Pay(10);

        Assert.AreEqual(10, paid);
        Assert.AreEqual(20, ledger.RemainingDebt);
        Assert.AreEqual(3, ledger.NextDebtDueRound);
    }

    [Test]
    public void Pay_FullPayment_AppliesMultiplierAndAdvancesDue()
    {
        DebtLedger ledger = new DebtLedger(3, 30, 2.2f);

        int paid = ledger.Pay(50);

        Assert.AreEqual(30, paid);
        Assert.AreEqual(66, ledger.RemainingDebt);
        Assert.AreEqual(6, ledger.NextDebtDueRound);
    }

    [Test]
    public void Pay_OverpayIsClampedToRemaining()
    {
        DebtLedger ledger = new DebtLedger(3, 30, 2.2f);

        int paid = ledger.Pay(100);

        Assert.AreEqual(30, paid);
    }

    [Test]
    public void Pay_ZeroOrNegative_ReturnsZeroAndKeepsState()
    {
        DebtLedger ledger = new DebtLedger(3, 30, 2.2f);

        int paid = ledger.Pay(-5) + ledger.Pay(0);

        Assert.AreEqual(0, paid);
        Assert.AreEqual(30, ledger.RemainingDebt);
        Assert.AreEqual(3, ledger.NextDebtDueRound);
    }

    [Test]
    public void MaxPayable_RoundsUntilDue_IsDue()
    {
        DebtLedger ledger = new DebtLedger(3, 30, 2.2f);

        Assert.AreEqual(30, ledger.MaxPayable(100));
        Assert.AreEqual(20, ledger.MaxPayable(20));
        Assert.AreEqual(3, ledger.RoundsUntilDue(0));
        Assert.IsFalse(ledger.IsDue(2));
        Assert.IsTrue(ledger.IsDue(3));
    }
}

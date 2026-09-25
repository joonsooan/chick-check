using UnityEngine;

public class DebtLedger
{
    private readonly int dueIntervalRounds;
    private readonly float debtGrowthOnPayoff;
    private float currentInstallmentTotal;

    public int RemainingDebt { get; private set; }
    public int NextDebtDueRound { get; private set; }

    public DebtLedger(int dueIntervalRounds, int initialDebt, float debtGrowthOnPayoff)
    {
        this.dueIntervalRounds = dueIntervalRounds;
        this.debtGrowthOnPayoff = debtGrowthOnPayoff;

        NextDebtDueRound = dueIntervalRounds;
        currentInstallmentTotal = initialDebt;
        RemainingDebt = Mathf.RoundToInt(currentInstallmentTotal);
    }

    public int Pay(int amount)
    {
        int payable = Mathf.Clamp(amount, 0, RemainingDebt);
        if (payable <= 0)
        {
            return 0;
        }

        RemainingDebt -= payable;
        if (RemainingDebt <= 0)
        {
            currentInstallmentTotal *= debtGrowthOnPayoff;
            RemainingDebt = Mathf.RoundToInt(currentInstallmentTotal);
            NextDebtDueRound += dueIntervalRounds;
        }

        return payable;
    }

    public int MaxPayable(int gold)
    {
        return Mathf.Max(0, Mathf.Min(RemainingDebt, gold));
    }

    public int RoundsUntilDue(int currentRound)
    {
        return Mathf.Max(NextDebtDueRound - currentRound, 0);
    }

    public bool IsDue(int currentRound)
    {
        return currentRound >= NextDebtDueRound;
    }
}

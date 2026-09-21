using UnityEngine;

public class DebtLedger
{
    private readonly int checkpointInterval;
    private readonly float multiplier;
    private float currentDebtAmount;

    public int RemainingDebt { get; private set; }
    public int NextDebtDueRound { get; private set; }

    public DebtLedger(int checkpointInterval, int initialAmount, float multiplier)
    {
        this.checkpointInterval = checkpointInterval;
        this.multiplier = multiplier;

        NextDebtDueRound = checkpointInterval;
        currentDebtAmount = initialAmount;
        RemainingDebt = Mathf.RoundToInt(currentDebtAmount);
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
            currentDebtAmount *= multiplier;
            RemainingDebt = Mathf.RoundToInt(currentDebtAmount);
            NextDebtDueRound += checkpointInterval;
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

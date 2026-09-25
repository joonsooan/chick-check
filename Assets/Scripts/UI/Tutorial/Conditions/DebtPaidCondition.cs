using System;
using UnityEngine;

[Serializable]
public class DebtPaidCondition : TutorialCondition
{
    [SerializeField, Min(1)] private int requiredAmount = 1;

    private int paidAmount;

    public override void Begin(TutorialContext context)
    {
        paidAmount = 0;
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnDebtPaid += HandleDebtPaid;
        }
    }

    public override bool IsMet(TutorialContext context) => paidAmount >= requiredAmount;

    public override void End(TutorialContext context)
    {
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnDebtPaid -= HandleDebtPaid;
        }
    }

    private void HandleDebtPaid(int amount) => paidAmount += amount;
}

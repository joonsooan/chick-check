using System;
using System.Collections.Generic;
using UnityEngine;

public class EconomyManager : Singleton<EconomyManager>
{
    [SerializeField] private EconomyConfigData economyConfig;
    [SerializeField] private DebtConfigData debtConfig;

    private DebtLedger debtLedger;
    private int minedGoldThisRound;

    public int Gold { get; private set; }
    public IReadOnlyList<SaleRecord> LastRoundSaleRecords { get; private set; }
    public int RemainingDebt => debtLedger?.RemainingDebt ?? 0;
    public int NextDebtDueRound => debtLedger?.NextDebtDueRound ?? 0;
    public int MaxPayableDebt => debtLedger != null ? debtLedger.MaxPayable(Gold) : 0;
    public bool IsDebtDueNow => GameManager.Instance != null && IsDebtDue(GameManager.Instance.CurrentRound);

    public event Action<int> OnGoldChanged;
    public event Action<IReadOnlyList<SaleRecord>> OnRoundResultsReady;
    public event Action<int, int> OnDebtStatusChanged;

    protected override void Awake()
    {
        base.Awake();

        if (economyConfig == null)
        {
            Debug.LogError("EconomyManager.economyConfig is not assigned.", this);
        }

        if (debtConfig == null)
        {
            Debug.LogError("EconomyManager.debtConfig is not assigned.", this);
        }

        Gold = economyConfig != null ? economyConfig.StartingGold : 0;
        debtLedger = debtConfig != null
            ? new DebtLedger(debtConfig.CheckpointInterval, debtConfig.Amount, debtConfig.Multiplier)
            : null;
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundEnded += HandleRoundEnded;
            GameManager.Instance.OnRoundStarted += HandleRoundStarted;
        }

        NotifyDebtStatus();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundEnded -= HandleRoundEnded;
            GameManager.Instance.OnRoundStarted -= HandleRoundStarted;
        }
    }

    private void HandleRoundStarted(int roundNumber)
    {
        minedGoldThisRound = 0;
        NotifyDebtStatus();
    }

    private void HandleRoundEnded(int roundNumber)
    {
        PlaceableEntity[] entities = new List<PlaceableEntity>(PlaceableEntity.ActiveEntities).ToArray();
        List<PlaceableEntity> sellable = new List<PlaceableEntity>();
        List<SaleInput> inputs = new List<SaleInput>();

        foreach (PlaceableEntity entity in entities)
        {
            if (entity == null || entity.IsCarried)
            {
                continue;
            }

            sellable.Add(entity);
            inputs.Add(new SaleInput(entity.EconomyData.Sprite, entity.EconomyData.SellPrice, entity.EconomyData.DisplayOrder, entity.SellPriceStat));
        }

        Func<ShopStatType, float, float> applyModifier =
            UpgradeManager.Instance != null ? (Func<ShopStatType, float, float>)UpgradeManager.Instance.ApplyModifiers : null;
        List<SaleRecord> saleRecords = RoundSettlement.Settle(inputs, applyModifier, out int totalGold);

        for (int i = 0; i < sellable.Count; i++)
        {
            sellable[i].HandleSold();
        }

        AddGold(totalGold);

        if (economyConfig != null)
        {
            RoundSettlement.AppendMinedGold(saleRecords, economyConfig.MinedGoldIcon, minedGoldThisRound, economyConfig.MinedGoldDisplayOrder);
        }

        minedGoldThisRound = 0;

        LastRoundSaleRecords = saleRecords;
        OnRoundResultsReady?.Invoke(saleRecords);
    }

    public bool IsDebtDue(int currentRound)
    {
        return debtLedger != null && debtLedger.IsDue(currentRound);
    }

    public void PayDebt(int amount)
    {
        if (debtLedger == null || GameManager.Instance == null || GameManager.Instance.CurrentPhase != GamePhase.Shop)
        {
            return;
        }

        int payable = Mathf.Clamp(amount, 0, MaxPayableDebt);
        if (payable <= 0 || !TrySpendGold(payable))
        {
            return;
        }

        debtLedger.Pay(payable);
        NotifyDebtStatus();
    }

    private void NotifyDebtStatus()
    {
        int currentRound = GameManager.Instance != null ? GameManager.Instance.CurrentRound : 0;
        int roundsUntilDue = debtLedger != null ? debtLedger.RoundsUntilDue(currentRound) : 0;
        OnDebtStatusChanged?.Invoke(RemainingDebt, roundsUntilDue);
    }

    public void AddGold(int amount)
    {
        Gold += amount;
        OnGoldChanged?.Invoke(Gold);
    }

    public void AddMinedGold(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        minedGoldThisRound += amount;
        AddGold(amount);
    }

    public bool TrySpendGold(int amount)
    {
        if (Gold < amount)
        {
            return false;
        }

        Gold -= amount;
        OnGoldChanged?.Invoke(Gold);
        return true;
    }
}

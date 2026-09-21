using System;
using System.Collections.Generic;
using UnityEngine;

public class EconomyManager : Singleton<EconomyManager>
{
    [SerializeField] private int startingGold = 10;

    public int Gold { get; private set; }
    public IReadOnlyList<SaleRecord> LastRoundSaleRecords { get; private set; }
    public event Action<int> OnGoldChanged;
    public event Action<IReadOnlyList<SaleRecord>> OnRoundResultsReady;

    protected override void Awake()
    {
        base.Awake();

        Gold = startingGold;
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundEnded += HandleRoundEnded;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundEnded -= HandleRoundEnded;
        }
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
        List<SaleRecord> saleRecords = RoundSettlement.Settle(inputs, applyModifier, out _);

        for (int i = 0; i < sellable.Count; i++)
        {
            AddGold(saleRecords[i].SellPrice);
            sellable[i].HandleSold();
        }

        LastRoundSaleRecords = saleRecords;
        OnRoundResultsReady?.Invoke(saleRecords);
    }

    public void AddGold(int amount)
    {
        Gold += amount;
        OnGoldChanged?.Invoke(Gold);
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

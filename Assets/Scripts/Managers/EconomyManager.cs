using System;
using System.Collections.Generic;
using UnityEngine;

public readonly struct SaleRecord
{
    public readonly Sprite Sprite;
    public readonly int SellPrice;
    public readonly int DisplayOrder;

    public SaleRecord(Sprite sprite, int sellPrice, int displayOrder)
    {
        Sprite = sprite;
        SellPrice = sellPrice;
        DisplayOrder = displayOrder;
    }
}

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
        List<SaleRecord> saleRecords = new List<SaleRecord>();

        foreach (PlaceableEntity entity in entities)
        {
            if (entity == null || entity.IsCarried)
            {
                continue;
            }

            saleRecords.Add(new SaleRecord(entity.EconomyData.Sprite, GetSellPrice(entity), entity.EconomyData.DisplayOrder));
            SellEntity(entity);
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

    public void SellEntity(PlaceableEntity entity)
    {
        AddGold(GetSellPrice(entity));
        entity.HandleSold();
    }

    private int GetSellPrice(PlaceableEntity entity)
    {
        int sellPrice = entity.EconomyData.SellPrice;
        if (UpgradeManager.Instance != null)
        {
            sellPrice = Mathf.RoundToInt(UpgradeManager.Instance.ApplyModifiers(entity.SellPriceStat, sellPrice));
        }

        return sellPrice;
    }
}

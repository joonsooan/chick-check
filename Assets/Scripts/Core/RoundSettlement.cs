using System;
using System.Collections.Generic;
using UnityEngine;

public readonly struct SaleInput
{
    public readonly Sprite Sprite;
    public readonly int BasePrice;
    public readonly int DisplayOrder;
    public readonly ShopStatType PriceStat;

    public SaleInput(Sprite sprite, int basePrice, int displayOrder, ShopStatType priceStat)
    {
        Sprite = sprite;
        BasePrice = basePrice;
        DisplayOrder = displayOrder;
        PriceStat = priceStat;
    }
}

public static class RoundSettlement
{
    public static List<SaleRecord> Settle(IReadOnlyList<SaleInput> inputs, Func<ShopStatType, float, float> applyModifier, out int totalGold)
    {
        List<SaleRecord> records = new List<SaleRecord>(inputs.Count);
        totalGold = 0;

        for (int i = 0; i < inputs.Count; i++)
        {
            SaleInput input = inputs[i];
            float modified = applyModifier != null ? applyModifier(input.PriceStat, input.BasePrice) : input.BasePrice;
            int price = Mathf.RoundToInt(modified);
            records.Add(new SaleRecord(input.Sprite, price, input.DisplayOrder));
            totalGold += price;
        }

        return records;
    }
}

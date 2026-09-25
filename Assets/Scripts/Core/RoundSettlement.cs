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

public readonly struct SaleGroup
{
    public readonly Sprite Sprite;
    public readonly int TotalPrice;
    public readonly int Count;
    public readonly int DisplayOrder;
    public readonly bool ShowCount;

    public SaleGroup(Sprite sprite, int totalPrice, int count, int displayOrder, bool showCount = true)
    {
        Sprite = sprite;
        TotalPrice = totalPrice;
        Count = count;
        DisplayOrder = displayOrder;
        ShowCount = showCount;
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

    public static List<SaleGroup> Summarize(IReadOnlyList<SaleRecord> records, out int totalGold)
    {
        List<Sprite> order = new List<Sprite>();
        Dictionary<Sprite, int> totalPriceBySprite = new Dictionary<Sprite, int>();
        Dictionary<Sprite, int> countBySprite = new Dictionary<Sprite, int>();
        Dictionary<Sprite, int> displayOrderBySprite = new Dictionary<Sprite, int>();
        Dictionary<Sprite, bool> showCountBySprite = new Dictionary<Sprite, bool>();

        totalGold = 0;

        for (int i = 0; i < records.Count; i++)
        {
            SaleRecord record = records[i];
            Sprite sprite = record.Sprite;

            if (!totalPriceBySprite.ContainsKey(sprite))
            {
                order.Add(sprite);
                totalPriceBySprite[sprite] = 0;
                countBySprite[sprite] = 0;
                displayOrderBySprite[sprite] = record.DisplayOrder;
                showCountBySprite[sprite] = record.ShowCount;
            }

            totalPriceBySprite[sprite] += record.SellPrice;
            countBySprite[sprite]++;
            totalGold += record.SellPrice;
        }

        StableSortByDisplayOrder(order, displayOrderBySprite);

        List<SaleGroup> groups = new List<SaleGroup>(order.Count);
        for (int i = 0; i < order.Count; i++)
        {
            Sprite sprite = order[i];
            groups.Add(new SaleGroup(sprite, totalPriceBySprite[sprite], countBySprite[sprite], displayOrderBySprite[sprite], showCountBySprite[sprite]));
        }

        return groups;
    }

    public static void AppendMinedGold(List<SaleRecord> records, Sprite icon, int amount, int displayOrder)
    {
        if (records == null || amount <= 0 || icon == null)
        {
            return;
        }

        records.Add(new SaleRecord(icon, amount, displayOrder, showCount: false));
    }

    private static void StableSortByDisplayOrder(List<Sprite> order, Dictionary<Sprite, int> displayOrder)
    {
        for (int i = 1; i < order.Count; i++)
        {
            Sprite current = order[i];
            int currentKey = displayOrder[current];
            int j = i - 1;
            while (j >= 0 && displayOrder[order[j]] > currentKey)
            {
                order[j + 1] = order[j];
                j--;
            }

            order[j + 1] = current;
        }
    }
}

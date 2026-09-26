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

    private readonly struct GroupKey : IEquatable<GroupKey>
    {
        public readonly Sprite Sprite;
        public readonly int DisplayOrder;

        public GroupKey(Sprite sprite, int displayOrder)
        {
            Sprite = sprite;
            DisplayOrder = displayOrder;
        }

        public bool Equals(GroupKey other) => Sprite == other.Sprite && DisplayOrder == other.DisplayOrder;
        public override bool Equals(object obj) => obj is GroupKey other && Equals(other);
        public override int GetHashCode() => ((Sprite != null ? Sprite.GetHashCode() : 0) * 397) ^ DisplayOrder;
    }

    public static List<SaleGroup> Summarize(IReadOnlyList<SaleRecord> records, out int totalGold)
    {
        List<GroupKey> order = new List<GroupKey>();
        Dictionary<GroupKey, int> totalPriceByKey = new Dictionary<GroupKey, int>();
        Dictionary<GroupKey, int> countByKey = new Dictionary<GroupKey, int>();
        Dictionary<GroupKey, bool> showCountByKey = new Dictionary<GroupKey, bool>();

        totalGold = 0;

        for (int i = 0; i < records.Count; i++)
        {
            SaleRecord record = records[i];
            GroupKey key = new GroupKey(record.Sprite, record.DisplayOrder);

            if (!totalPriceByKey.ContainsKey(key))
            {
                order.Add(key);
                totalPriceByKey[key] = 0;
                countByKey[key] = 0;
                showCountByKey[key] = record.ShowCount;
            }

            totalPriceByKey[key] += record.SellPrice;
            countByKey[key]++;
            totalGold += record.SellPrice;
        }

        StableSortByDisplayOrder(order);

        List<SaleGroup> groups = new List<SaleGroup>(order.Count);
        for (int i = 0; i < order.Count; i++)
        {
            GroupKey key = order[i];
            groups.Add(new SaleGroup(key.Sprite, totalPriceByKey[key], countByKey[key], key.DisplayOrder, showCountByKey[key]));
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

    private static void StableSortByDisplayOrder(List<GroupKey> order)
    {
        for (int i = 1; i < order.Count; i++)
        {
            GroupKey current = order[i];
            int j = i - 1;
            while (j >= 0 && order[j].DisplayOrder > current.DisplayOrder)
            {
                order[j + 1] = order[j];
                j--;
            }

            order[j + 1] = current;
        }
    }
}

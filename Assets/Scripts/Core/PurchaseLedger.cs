using System.Collections.Generic;
using UnityEngine;

public class PurchaseLedger
{
    private readonly Dictionary<ShopItemData, int> purchaseCounts = new Dictionary<ShopItemData, int>();
    private readonly HashSet<ShopItemData> soldOutItems = new HashSet<ShopItemData>();

    public int GetCurrentCost(ShopItemData item)
    {
        purchaseCounts.TryGetValue(item, out int count);
        return Mathf.RoundToInt(item.Cost * Mathf.Pow(item.CostGrowthPerPurchase, count));
    }

    public void RecordPurchase(ShopItemData item)
    {
        purchaseCounts.TryGetValue(item, out int count);
        purchaseCounts[item] = count + 1;

        if (item.OneTimeOnly)
        {
            soldOutItems.Add(item);
        }
    }

    public bool IsSoldOut(ShopItemData item)
    {
        return soldOutItems.Contains(item);
    }

    public void Clear()
    {
        purchaseCounts.Clear();
        soldOutItems.Clear();
    }
}

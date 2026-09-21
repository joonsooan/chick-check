using System.Collections.Generic;
using UnityEngine;

public class PurchaseLedger
{
    private readonly Dictionary<ShopItemData, int> purchaseCounts = new Dictionary<ShopItemData, int>();
    private readonly HashSet<ShopItemData> retiredItems = new HashSet<ShopItemData>();

    public int GetCurrentCost(ShopItemData item)
    {
        purchaseCounts.TryGetValue(item, out int count);
        return Mathf.RoundToInt(item.Cost * Mathf.Pow(item.PriceMultiplier, count));
    }

    public void RecordPurchase(ShopItemData item)
    {
        purchaseCounts.TryGetValue(item, out int count);
        purchaseCounts[item] = count + 1;

        if (item.OneTimeOnly)
        {
            retiredItems.Add(item);
        }
    }

    public bool IsRetired(ShopItemData item)
    {
        return retiredItems.Contains(item);
    }

    public void Clear()
    {
        purchaseCounts.Clear();
        retiredItems.Clear();
    }
}

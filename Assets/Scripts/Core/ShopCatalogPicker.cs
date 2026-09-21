using System;
using System.Collections.Generic;

public static class ShopCatalogPicker
{
    public static List<ShopItemData> Pick(
        IReadOnlyList<ShopItemData> all,
        int count,
        Func<ShopItemData, bool> isAvailable,
        Random rng)
    {
        List<ShopItemData> pool = new List<ShopItemData>();
        foreach (ShopItemData item in all)
        {
            if (isAvailable(item))
            {
                pool.Add(item);
            }
        }

        int pickCount = Math.Min(count, pool.Count);
        List<ShopItemData> result = new List<ShopItemData>(pickCount);

        for (int i = 0; i < pickCount; i++)
        {
            int index = rng.Next(pool.Count);
            result.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return result;
    }
}

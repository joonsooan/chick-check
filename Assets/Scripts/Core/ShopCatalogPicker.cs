using System;
using System.Collections.Generic;

public static class ShopCatalogPicker
{
    public static List<ShopItemData> Pick(
        IReadOnlyList<ShopItemData> all,
        int count,
        Func<ShopItemData, bool> isAvailable,
        Random rng,
        IReadOnlyList<ShopItemData> pinned = null)
    {
        List<ShopItemData> result = new List<ShopItemData>();
        HashSet<ShopItemData> used = new HashSet<ShopItemData>();

        if (pinned != null)
        {
            for (int i = 0; i < pinned.Count && result.Count < count; i++)
            {
                ShopItemData item = pinned[i];
                if (item != null && isAvailable(item) && used.Add(item))
                {
                    result.Add(item);
                }
            }
        }

        List<ShopItemData> pool = new List<ShopItemData>();
        foreach (ShopItemData item in all)
        {
            if (!used.Contains(item) && isAvailable(item))
            {
                pool.Add(item);
            }
        }

        int remaining = Math.Min(count - result.Count, pool.Count);

        for (int i = 0; i < remaining; i++)
        {
            int index = rng.Next(pool.Count);
            result.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return result;
    }
}

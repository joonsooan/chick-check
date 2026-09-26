using System;
using System.Collections.Generic;

public class ShopUnlockTracker
{
    private readonly HashSet<ShopItemData> seen = new HashSet<ShopItemData>();

    public List<ShopItemData> CollectNewlyUnlocked(IReadOnlyList<ShopItemData> all, Func<ShopItemData, bool> isAvailable)
    {
        List<ShopItemData> newlyUnlocked = new List<ShopItemData>();

        for (int i = 0; i < all.Count; i++)
        {
            ShopItemData item = all[i];
            if (item == null || !isAvailable(item))
            {
                continue;
            }

            if (!seen.Contains(item) && item.PinWhenUnlocked)
            {
                newlyUnlocked.Add(item);
            }

            seen.Add(item);
        }

        return newlyUnlocked;
    }

    public void Clear()
    {
        seen.Clear();
    }
}

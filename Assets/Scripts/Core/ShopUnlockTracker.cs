using System;
using System.Collections.Generic;

public class ShopUnlockTracker
{
    private readonly HashSet<ShopItemData> seen = new HashSet<ShopItemData>();
    private readonly List<ShopItemData> pinned = new List<ShopItemData>();

    public IReadOnlyList<ShopItemData> Pinned => pinned;

    public void BeginShopPhase(IReadOnlyList<ShopItemData> all, Func<ShopItemData, bool> isAvailable)
    {
        pinned.RemoveAll(item => !item.OneTimeOnly || !isAvailable(item));

        for (int i = 0; i < all.Count; i++)
        {
            ShopItemData item = all[i];
            if (item == null || !isAvailable(item))
            {
                continue;
            }

            if (!seen.Contains(item) && item.PinWhenUnlocked)
            {
                pinned.Add(item);
            }

            seen.Add(item);
        }
    }

    public void RefreshPinned(Func<ShopItemData, bool> isAvailable)
    {
        pinned.RemoveAll(item => !isAvailable(item));
    }

    public void Clear()
    {
        seen.Clear();
        pinned.Clear();
    }
}

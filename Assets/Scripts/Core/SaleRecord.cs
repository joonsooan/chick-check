using UnityEngine;

public readonly struct SaleRecord
{
    public readonly Sprite Sprite;
    public readonly int SellPrice;
    public readonly int DisplayOrder;
    public readonly bool ShowCount;

    public SaleRecord(Sprite sprite, int sellPrice, int displayOrder, bool showCount = true)
    {
        Sprite = sprite;
        SellPrice = sellPrice;
        DisplayOrder = displayOrder;
        ShowCount = showCount;
    }
}

using UnityEngine;

public readonly struct SaleRecord
{
    public readonly Sprite Sprite;
    public readonly int SellPrice;
    public readonly int DisplayOrder;

    public SaleRecord(Sprite sprite, int sellPrice, int displayOrder)
    {
        Sprite = sprite;
        SellPrice = sellPrice;
        DisplayOrder = displayOrder;
    }
}

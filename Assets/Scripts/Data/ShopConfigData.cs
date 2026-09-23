using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopConfigData", menuName = "Chick Check/Shop Config")]
public class ShopConfigData : ScriptableObject
{
    [SerializeField, Min(0)] private int rerollCost = 10;
    [SerializeField, Min(1)] private int itemsPerRound = 3;
    [SerializeField] private List<ShopItemData> catalog = new List<ShopItemData>();

    public int RerollCost => rerollCost;
    public int ItemsPerRound => itemsPerRound;
    public IReadOnlyList<ShopItemData> Catalog => catalog;
}

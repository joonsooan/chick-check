using UnityEngine;

[CreateAssetMenu(fileName = "EconomyConfigData", menuName = "Chick Colony/Economy Config")]
public class EconomyConfigData : ScriptableObject
{
    [SerializeField, Min(0)] private int startingGold = 10;
    [SerializeField] private Sprite minedGoldIcon;
    [SerializeField] private int minedGoldDisplayOrder = 100;

    public int StartingGold => startingGold;
    public Sprite MinedGoldIcon => minedGoldIcon;
    public int MinedGoldDisplayOrder => minedGoldDisplayOrder;
}

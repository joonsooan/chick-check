using UnityEngine;

public enum ShopStatType
{
    ChickSpawnCountPerRound,
    PlayerMoveSpeed,
    SellPrice,
    GrowthInterval,
    GrowthChance,
    RoundDuration
}

public enum ShopModifierOp
{
    Add,
    Multiply
}

[CreateAssetMenu(fileName = "ShopItemData", menuName = "Shop Item Data")]
public class ShopItemData : ScriptableObject
{
    [SerializeField] private string itemName;
    [TextArea] [SerializeField] private string description;
    [SerializeField] private Sprite icon;
    [SerializeField] private int cost = 10;
    [SerializeField] private ShopStatType targetStat;
    [SerializeField] private ShopModifierOp op;
    [SerializeField] private float value = 1f;

    public string ItemName => itemName;
    public string Description => description;
    public Sprite Icon => icon;
    public int Cost => cost;
    public ShopStatType TargetStat => targetStat;
    public ShopModifierOp Op => op;
    public float Value => value;
}

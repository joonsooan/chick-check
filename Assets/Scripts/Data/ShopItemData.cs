using UnityEngine;

public enum ShopStatType
{
    ChickSpawnCountPerRound,
    PlayerMoveSpeed,
    SellPrice,
    GrowthInterval,
    GrowthChance,
    RoundDuration,
    EggLayInterval,
    EggLayChance
}

public enum ShopModifierOp
{
    Add,
    Multiply
}

public enum ShopEffectKind
{
    StatModifier,
    EnableMutation,
    EnableContagion,
    MirrorUpgrade
}

[CreateAssetMenu(fileName = "ShopItemData", menuName = "Shop Item Data")]
public class ShopItemData : ScriptableObject
{
    [SerializeField] private string itemName;
    [TextArea][SerializeField] private string description;
    [SerializeField] private Sprite icon;
    [SerializeField] private int cost = 10;
    [SerializeField] private ShopEffectKind effectKind;
    [SerializeField] private bool oneTimeOnly;

    [Header("Stat Modifier")]
    [SerializeField] private ShopStatType targetStat;
    [SerializeField] private ShopModifierOp op;
    [SerializeField] private float value = 1f;
    [SerializeField] private float priceMultiplier = 2f;

    [Header("Mirror Upgrade")]
    [SerializeField] private GameObject mirrorReplacementPrefab;

    public string ItemName => itemName;
    public string Description => description;
    public Sprite Icon => icon;
    public int Cost => cost;
    public ShopEffectKind EffectKind => effectKind;
    public bool OneTimeOnly => oneTimeOnly;
    public ShopStatType TargetStat => targetStat;
    public ShopModifierOp Op => op;
    public float Value => value;
    public float PriceMultiplier => priceMultiplier;
    public GameObject MirrorReplacementPrefab => mirrorReplacementPrefab;
}

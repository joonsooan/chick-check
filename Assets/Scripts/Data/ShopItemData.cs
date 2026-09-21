using UnityEngine;

public enum ShopStatType
{
    ChickSpawnCountPerRound,
    PlayerMoveSpeed,
    SellPriceChick,
    GrowthChance,
    RoundDuration,
    EggLayChance,
    EggGrowthChance,
    SellPriceEgg,
    SellPriceChicken
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
    MirrorUpgrade,
    EnableSprint,
    GoldMine,
    MultiLay
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

    [Header("Gold Mine")]
    [SerializeField] private float goldMineInterval = 5f;
    [SerializeField] private float goldMineChance = 0.5f;
    [SerializeField] private int goldMineBaseGold = 1;
    [SerializeField] private int goldMineGoldPerPurchase = 1;

    [Header("Multi Lay")]
    [SerializeField] private float multiLayChance = 0.5f;
    [SerializeField] private int multiLayCountPerPurchase = 1;

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
    public float GoldMineInterval => goldMineInterval;
    public float GoldMineChance => goldMineChance;
    public int GoldMineBaseGold => goldMineBaseGold;
    public int GoldMineGoldPerPurchase => goldMineGoldPerPurchase;
    public float MultiLayChance => multiLayChance;
    public int MultiLayCountPerPurchase => multiLayCountPerPurchase;

#if UNITY_INCLUDE_TESTS
    internal static ShopItemData CreateForTest(
        string itemName = "Test Item",
        int cost = 10,
        float priceMultiplier = 2f,
        bool oneTimeOnly = false,
        ShopEffectKind effectKind = ShopEffectKind.StatModifier,
        ShopStatType targetStat = ShopStatType.GrowthChance,
        ShopModifierOp op = ShopModifierOp.Add,
        float value = 1f)
    {
        ShopItemData instance = CreateInstance<ShopItemData>();
        instance.itemName = itemName;
        instance.cost = cost;
        instance.priceMultiplier = priceMultiplier;
        instance.oneTimeOnly = oneTimeOnly;
        instance.effectKind = effectKind;
        instance.targetStat = targetStat;
        instance.op = op;
        instance.value = value;
        return instance;
    }
#endif
}

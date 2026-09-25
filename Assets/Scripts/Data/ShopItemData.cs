using UnityEngine;

[CreateAssetMenu(fileName = "ShopItemData", menuName = "Chick Check/Shop Item Data")]
public class ShopItemData : ScriptableObject
{
    [SerializeField] private string itemName;
    [SerializeField, TextArea] private string description;
    [SerializeField] private Sprite icon;
    [SerializeField] private int cost = 10;
    [SerializeField] private float priceMultiplier = 2f;
    [SerializeField] private bool oneTimeOnly;

    [SerializeReference] private ShopEffect effect;

    public string ItemName => itemName;
    public string Description => description;
    public Sprite Icon => icon;
    public int Cost => cost;
    public float CostGrowthPerPurchase => priceMultiplier;
    public bool OneTimeOnly => oneTimeOnly;
    public ShopEffect Effect => effect;

#if UNITY_INCLUDE_TESTS
    internal static ShopItemData CreateForTest(
        string itemName = "Test Item",
        int cost = 10,
        float costGrowthPerPurchase = 2f,
        bool oneTimeOnly = false,
        ShopEffect effect = null)
    {
        ShopItemData instance = CreateInstance<ShopItemData>();
        instance.itemName = itemName;
        instance.cost = cost;
        instance.priceMultiplier = costGrowthPerPurchase;
        instance.oneTimeOnly = oneTimeOnly;
        instance.effect = effect ?? new StatModifierEffect();
        return instance;
    }
#endif
}

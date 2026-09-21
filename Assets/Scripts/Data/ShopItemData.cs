using UnityEngine;

[CreateAssetMenu(fileName = "ShopItemData", menuName = "Shop Item Data")]
public class ShopItemData : ScriptableObject
{
    [SerializeField] private string itemName;
    [TextArea][SerializeField] private string description;
    [SerializeField] private Sprite icon;
    [SerializeField] private int cost = 10;
    [SerializeField] private float priceMultiplier = 2f;
    [SerializeField] private bool oneTimeOnly;

    [SerializeReference] private ShopEffect effect;

    public string ItemName => itemName;
    public string Description => description;
    public Sprite Icon => icon;
    public int Cost => cost;
    public float PriceMultiplier => priceMultiplier;
    public bool OneTimeOnly => oneTimeOnly;
    public ShopEffect Effect => effect;

#if UNITY_INCLUDE_TESTS
    internal static ShopItemData CreateForTest(
        string itemName = "Test Item",
        int cost = 10,
        float priceMultiplier = 2f,
        bool oneTimeOnly = false,
        ShopEffect effect = null)
    {
        ShopItemData instance = CreateInstance<ShopItemData>();
        instance.itemName = itemName;
        instance.cost = cost;
        instance.priceMultiplier = priceMultiplier;
        instance.oneTimeOnly = oneTimeOnly;
        instance.effect = effect ?? new StatModifierEffect();
        return instance;
    }
#endif
}

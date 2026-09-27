using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "ShopItemData", menuName = "Chick Check/Shop Item Data")]
public class ShopItemData : ScriptableObject
{
    [SerializeField] private LocalizedString itemName = new LocalizedString();
    [SerializeField] private LocalizedString description = new LocalizedString();
    [SerializeField] private Sprite icon;
    [SerializeField] private int cost = 10;
    [SerializeField] private float priceMultiplier = 2f;
    [SerializeField] private bool oneTimeOnly;
    [SerializeField] private bool pinWhenUnlocked;
    [SerializeField] private List<EconomyEntityData> requiredSightings = new List<EconomyEntityData>();

    [SerializeReference] private ShopEffect effect;

    public LocalizedString ItemName => itemName;
    public LocalizedString Description => description;
    public Sprite Icon => icon;
    public int Cost => cost;
    public float CostGrowthPerPurchase => priceMultiplier;
    public bool OneTimeOnly => oneTimeOnly;
    public bool PinWhenUnlocked => pinWhenUnlocked;
    public IReadOnlyList<EconomyEntityData> RequiredSightings => requiredSightings;
    public ShopEffect Effect => effect;

#if UNITY_INCLUDE_TESTS
    internal static ShopItemData CreateForTest(
        string itemName = "Test Item",
        int cost = 10,
        float costGrowthPerPurchase = 2f,
        bool oneTimeOnly = false,
        bool pinWhenUnlocked = false,
        ShopEffect effect = null,
        List<EconomyEntityData> requiredSightings = null)
    {
        ShopItemData instance = CreateInstance<ShopItemData>();
        instance.name = itemName;
        instance.cost = cost;
        instance.priceMultiplier = costGrowthPerPurchase;
        instance.oneTimeOnly = oneTimeOnly;
        instance.pinWhenUnlocked = pinWhenUnlocked;
        instance.effect = effect ?? new StatModifierEffect();
        instance.requiredSightings = requiredSightings ?? new List<EconomyEntityData>();
        return instance;
    }
#endif
}

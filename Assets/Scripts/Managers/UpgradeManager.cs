using System;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [SerializeField] private ShopItemData[] shopCatalog;

    public event Action<IReadOnlyList<ShopItemData>> OnShopCatalogReady;
    public event Action<ShopItemData> OnItemPurchased;
    public event Action<ShopItemData, string> OnPurchaseFailed;

    private readonly Dictionary<ShopStatType, float> additiveBonus = new Dictionary<ShopStatType, float>();
    private readonly Dictionary<ShopStatType, float> multiplier = new Dictionary<ShopStatType, float>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPhaseChanged += HandlePhaseChanged;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPhaseChanged -= HandlePhaseChanged;
        }
    }

    private void HandlePhaseChanged(GamePhase phase)
    {
        if (phase == GamePhase.Shop)
        {
            OnShopCatalogReady?.Invoke(shopCatalog);
        }
    }

    public bool TryPurchase(ShopItemData item)
    {
        if (!EconomyManager.Instance.TrySpendGold(item.Cost))
        {
            OnPurchaseFailed?.Invoke(item, "gold");
            return false;
        }

        if (item.Op == ShopModifierOp.Add)
        {
            additiveBonus.TryGetValue(item.TargetStat, out float current);
            additiveBonus[item.TargetStat] = current + item.Value;
        }
        else
        {
            multiplier.TryGetValue(item.TargetStat, out float current);
            multiplier[item.TargetStat] = (current == 0f ? 1f : current) * item.Value;
        }

        OnItemPurchased?.Invoke(item);
        return true;
    }

    public float GetMultiplier(ShopStatType stat)
    {
        return multiplier.TryGetValue(stat, out float value) ? value : 1f;
    }

    public float GetAdditive(ShopStatType stat)
    {
        return additiveBonus.TryGetValue(stat, out float value) ? value : 0f;
    }

    public float ApplyModifiers(ShopStatType stat, float baseValue)
    {
        return (baseValue + GetAdditive(stat)) * GetMultiplier(stat);
    }
}

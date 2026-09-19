using System;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : Singleton<UpgradeManager>
{
    [SerializeField] private string shopItemsResourcePath = "Shop Items";
    [SerializeField] private int shopItemsPerRound = 3;

    public event Action<IReadOnlyList<ShopItemData>> OnShopCatalogReady;

    private readonly List<ShopItemData> allShopItems = new List<ShopItemData>();
    private readonly Dictionary<ShopStatType, float> additiveBonus = new Dictionary<ShopStatType, float>();
    private readonly Dictionary<ShopStatType, float> multiplier = new Dictionary<ShopStatType, float>();
    private readonly Dictionary<ShopItemData, int> purchaseCounts = new Dictionary<ShopItemData, int>();

    protected override void Awake()
    {
        base.Awake();

        allShopItems.AddRange(Resources.LoadAll<ShopItemData>(shopItemsResourcePath));
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPhaseChanged += HandlePhaseChanged;
            GameManager.Instance.OnGameOver += HandleGameOver;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPhaseChanged -= HandlePhaseChanged;
            GameManager.Instance.OnGameOver -= HandleGameOver;
        }
    }

    private void HandleGameOver(GameOverReason reason)
    {
        purchaseCounts.Clear();
    }

    private void HandlePhaseChanged(GamePhase phase)
    {
        if (phase == GamePhase.Shop)
        {
            OnShopCatalogReady?.Invoke(PickRandomItems());
        }
    }

    public int RerollCost => GameManager.Instance != null ? GameManager.Instance.RerollCost : 0;

    public bool TryReroll()
    {
        if (EconomyManager.Instance == null || !EconomyManager.Instance.TrySpendGold(RerollCost))
        {
            return false;
        }

        OnShopCatalogReady?.Invoke(PickRandomItems());
        return true;
    }

    private List<ShopItemData> PickRandomItems()
    {
        List<ShopItemData> pool = new List<ShopItemData>(allShopItems);
        int pickCount = Mathf.Min(shopItemsPerRound, pool.Count);
        List<ShopItemData> result = new List<ShopItemData>(pickCount);

        for (int i = 0; i < pickCount; i++)
        {
            int index = UnityEngine.Random.Range(0, pool.Count);
            result.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return result;
    }

    public bool TryPurchase(ShopItemData item)
    {
        if (!EconomyManager.Instance.TrySpendGold(GetCurrentCost(item)))
        {
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

        purchaseCounts.TryGetValue(item, out int count);
        purchaseCounts[item] = count + 1;

        return true;
    }

    public int GetCurrentCost(ShopItemData item)
    {
        purchaseCounts.TryGetValue(item, out int count);
        return Mathf.RoundToInt(item.Cost * Mathf.Pow(item.PriceMultiplier, count));
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

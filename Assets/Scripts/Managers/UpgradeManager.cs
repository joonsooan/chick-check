using System;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : Singleton<UpgradeManager>
{
    [SerializeField] private string shopItemsResourcePath = "Shop Items";
    [SerializeField] private int shopItemsPerRound = 3;
    [SerializeField] private MutationData mutationData;

    public event Action<IReadOnlyList<ShopItemData>> OnShopCatalogReady;

    public event Action OnModifiersChanged;

    public event Action<GameObject> OnMirrorUpgrade;

    private readonly List<ShopItemData> allShopItems = new List<ShopItemData>();
    private readonly Dictionary<ShopStatType, float> additiveBonus = new Dictionary<ShopStatType, float>();
    private readonly Dictionary<ShopStatType, float> multiplier = new Dictionary<ShopStatType, float>();
    private readonly Dictionary<ShopItemData, int> purchaseCounts = new Dictionary<ShopItemData, int>();
    private readonly HashSet<ShopItemData> retiredItems = new HashSet<ShopItemData>();

    public MutationData MutationData => mutationData;
    public int MutantSpawnCount { get; private set; }
    public bool ContagionEnabled { get; private set; }
    public bool SprintEnabled { get; private set; }

    public int GoldMineLevel { get; private set; }
    public float GoldMineInterval { get; private set; }
    public float GoldMineChance { get; private set; }
    private int goldMineBaseGold;
    private int goldMineGoldPerPurchase;
    public int GoldMineGoldAmount => GoldMineLevel <= 0 ? 0 : goldMineBaseGold + goldMineGoldPerPurchase * (GoldMineLevel - 1);

    public float MultiLayChance { get; private set; }
    public int MultiLayBonusCount { get; private set; }

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

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPhaseChanged -= HandlePhaseChanged;
            GameManager.Instance.OnGameOver -= HandleGameOver;
        }
    }

    private void HandleGameOver(GameOverReason reason)
    {
        purchaseCounts.Clear();
        retiredItems.Clear();
        MutantSpawnCount = 0;
        ContagionEnabled = false;
        SprintEnabled = false;
        GoldMineLevel = 0;
        GoldMineInterval = 0f;
        GoldMineChance = 0f;
        goldMineBaseGold = 0;
        goldMineGoldPerPurchase = 0;
        MultiLayChance = 0f;
        MultiLayBonusCount = 0;
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
        List<ShopItemData> pool = new List<ShopItemData>();
        foreach (ShopItemData item in allShopItems)
        {
            if (!retiredItems.Contains(item))
            {
                pool.Add(item);
            }
        }
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

        switch (item.EffectKind)
        {
            case ShopEffectKind.StatModifier:
                if (item.Op == ShopModifierOp.Add)
                {
                    additiveBonus.TryGetValue(item.TargetStat, out float current);
                    additiveBonus[item.TargetStat] = current + item.Value;
                }
                else
                {
                    multiplier.TryGetValue(item.TargetStat, out float current);
                    multiplier[item.TargetStat] = (current == 0f ? 1f : current) + (item.Value - 1f);
                }
                break;
            case ShopEffectKind.EnableMutation:
                MutantSpawnCount++;
                break;
            case ShopEffectKind.EnableContagion:
                ContagionEnabled = true;
                break;
            case ShopEffectKind.MirrorUpgrade:
                OnMirrorUpgrade?.Invoke(item.MirrorReplacementPrefab);
                break;
            case ShopEffectKind.EnableSprint:
                SprintEnabled = true;
                break;
            case ShopEffectKind.GoldMine:
                GoldMineInterval = item.GoldMineInterval;
                GoldMineChance = item.GoldMineChance;
                goldMineBaseGold = item.GoldMineBaseGold;
                goldMineGoldPerPurchase = item.GoldMineGoldPerPurchase;
                GoldMineLevel++;
                break;
            case ShopEffectKind.MultiLay:
                MultiLayChance = item.MultiLayChance;
                MultiLayBonusCount += item.MultiLayCountPerPurchase;
                break;
        }

        if (item.OneTimeOnly)
        {
            retiredItems.Add(item);
        }

        purchaseCounts.TryGetValue(item, out int count);
        purchaseCounts[item] = count + 1;

        OnModifiersChanged?.Invoke();

        return true;
    }

    public int GetCurrentCost(ShopItemData item)
    {
        purchaseCounts.TryGetValue(item, out int count);
        return Mathf.RoundToInt(item.Cost * Mathf.Pow(item.PriceMultiplier, count));
    }

    public float GetAdditive(ShopStatType stat)
    {
        return additiveBonus.TryGetValue(stat, out float value) ? value : 0f;
    }

    public float GetMultiplier(ShopStatType stat)
    {
        return multiplier.TryGetValue(stat, out float value) ? value : 1f;
    }

    public float ApplyModifiers(ShopStatType stat, float baseValue)
    {
        return (baseValue + GetAdditive(stat)) * GetMultiplier(stat);
    }
}

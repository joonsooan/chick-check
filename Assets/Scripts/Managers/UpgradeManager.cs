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
    private readonly StatModifierTable modifiers = new StatModifierTable();
    private readonly PurchaseLedger ledger = new PurchaseLedger();
    private readonly System.Random rng = new System.Random();

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
        ledger.Clear();
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
        return ShopCatalogPicker.Pick(allShopItems, shopItemsPerRound, IsAvailable, rng);
    }

    private bool IsAvailable(ShopItemData item)
    {
        return !ledger.IsRetired(item)
            && (item.EffectKind != ShopEffectKind.EnableContagion || MutantSpawnCount > 0);
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
                    modifiers.AddAdditive(item.TargetStat, item.Value);
                }
                else
                {
                    modifiers.AddMultiplier(item.TargetStat, item.Value);
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

        ledger.RecordPurchase(item);

        OnModifiersChanged?.Invoke();

        return true;
    }

    public int GetCurrentCost(ShopItemData item)
    {
        return ledger.GetCurrentCost(item);
    }

    public float GetAdditive(ShopStatType stat)
    {
        return modifiers.GetAdditive(stat);
    }

    public float GetMultiplier(ShopStatType stat)
    {
        return modifiers.GetMultiplier(stat);
    }

    public float ApplyModifiers(ShopStatType stat, float baseValue)
    {
        return modifiers.Apply(stat, baseValue);
    }
}

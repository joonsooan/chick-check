using System;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : Singleton<UpgradeManager>
{
    [SerializeField] private MutationData mutationData;
    [SerializeField] private ShopConfigData shopConfig;
    [SerializeField] private ChickMergeData chickMergeData;
    [SerializeField] private List<ShopItemData> debugStartingItems = new List<ShopItemData>();

    public event Action<IReadOnlyList<ShopItemData>> OnShopCatalogReady;

    public event Action OnModifiersChanged;

    public event Action<GameObject> OnMirrorUpgrade;

    private readonly List<ShopItemData> allShopItems = new List<ShopItemData>();
    private readonly UpgradeState state = new UpgradeState();
    private readonly PurchaseLedger purchaseLedger = new PurchaseLedger();
    private readonly ShopUnlockTracker unlockTracker = new ShopUnlockTracker();
    private readonly System.Random rng = new System.Random();
    private List<ShopItemData> pinnedForRound = new List<ShopItemData>();

    public UpgradeState State => state;

    public MutationData MutationData => mutationData;

    protected override void Awake()
    {
        base.Awake();

        if (shopConfig == null)
        {
            Debug.LogError("UpgradeManager.shopConfig is not assigned.", this);
        }
        else
        {
            foreach (ShopItemData item in shopConfig.Catalog)
            {
                if (item != null)
                {
                    allShopItems.Add(item);
                }
            }
        }
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPhaseChanged += HandlePhaseChanged;
            GameManager.Instance.OnGameOver += HandleGameOver;
            GameManager.Instance.OnShopPhaseEnding += HandleShopPhaseEnding;
        }

        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.OnEntityCountChanged += HandleEntityCountChanged;
        }

        GrantDebugStartingItems();
    }

    private void GrantDebugStartingItems()
    {
        if (!Application.isEditor && !Debug.isDebugBuild)
        {
            return;
        }

        bool granted = false;
        foreach (ShopItemData item in debugStartingItems)
        {
            if (item == null || item.Effect == null)
            {
                continue;
            }

            ApplyPurchase(item);
            granted = true;
        }

        if (granted)
        {
            OnModifiersChanged?.Invoke();
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPhaseChanged -= HandlePhaseChanged;
            GameManager.Instance.OnGameOver -= HandleGameOver;
            GameManager.Instance.OnShopPhaseEnding -= HandleShopPhaseEnding;
        }

        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.OnEntityCountChanged -= HandleEntityCountChanged;
        }
    }

    private void HandleGameOver(GameOverReason reason)
    {
        purchaseLedger.Clear();
        state.Reset();
        unlockTracker.Clear();
        pinnedForRound.Clear();
    }

    private void HandleShopPhaseEnding(int roundNumber)
    {
        pinnedForRound.Clear();
    }

    private void HandleEntityCountChanged(EconomyEntityData data, int count)
    {
        if (chickMergeData == null || SpawnManager.Instance == null)
        {
            return;
        }

        IReadOnlyList<ChickMergeData.MergeRecipe> recipes = chickMergeData.Recipes;
        for (int i = 0; i < recipes.Count; i++)
        {
            ChickMergeData.MergeRecipe recipe = recipes[i];
            if (!ContainsSource(recipe, data))
            {
                continue;
            }

            int sum = SumSourceCounts(recipe);
            state.Get<ChickMergeState>().RecordCount(i, sum);
        }
    }

    private static bool ContainsSource(ChickMergeData.MergeRecipe recipe, EconomyEntityData data)
    {
        IReadOnlyList<EconomyEntityData> sourceData = recipe.SourceData;
        for (int i = 0; i < sourceData.Count; i++)
        {
            if (sourceData[i] == data)
            {
                return true;
            }
        }

        return false;
    }

    private static int SumSourceCounts(ChickMergeData.MergeRecipe recipe)
    {
        int sum = 0;
        IReadOnlyList<EconomyEntityData> sourceData = recipe.SourceData;
        for (int i = 0; i < sourceData.Count; i++)
        {
            sum += SpawnManager.Instance.CountOf(sourceData[i]);
        }

        return sum;
    }

    private void HandlePhaseChanged(GamePhase phase)
    {
        if (phase == GamePhase.Shop)
        {
            pinnedForRound = unlockTracker.CollectNewlyUnlocked(allShopItems, IsAvailable);
            OnShopCatalogReady?.Invoke(PickRandomItems());
        }
    }

    public int RerollCost => shopConfig != null ? shopConfig.RerollCost : 0;

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
        int itemsPerRound = shopConfig != null ? shopConfig.ItemsPerRound : 0;
        return ShopCatalogPicker.Pick(allShopItems, itemsPerRound, IsAvailable, rng, pinnedForRound);
    }

    private bool IsAvailable(ShopItemData item)
    {
        return !purchaseLedger.IsSoldOut(item)
            && item.Effect != null
            && item.Effect.IsAvailable(state);
    }

    public bool TryPurchase(ShopItemData item)
    {
        if (item.Effect == null)
        {
            return false;
        }

        if (!EconomyManager.Instance.TrySpendGold(GetCurrentCost(item)))
        {
            return false;
        }

        ApplyPurchase(item);

        OnModifiersChanged?.Invoke();

        return true;
    }

    private void ApplyPurchase(ShopItemData item)
    {
        item.Effect.Apply(state);

        GameObject mirrorPrefab = state.ConsumePendingMirrorPrefab();
        if (mirrorPrefab != null)
        {
            OnMirrorUpgrade?.Invoke(mirrorPrefab);
        }

        purchaseLedger.RecordPurchase(item);
    }

    public int GetCurrentCost(ShopItemData item)
    {
        return purchaseLedger.GetCurrentCost(item);
    }

    public float ApplyModifiers(ShopStatType stat, float baseValue)
    {
        return state.Modifiers.Apply(stat, baseValue);
    }
}

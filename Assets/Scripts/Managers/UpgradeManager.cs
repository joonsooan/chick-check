using System;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : Singleton<UpgradeManager>
{
    [SerializeField] private string shopItemsResourcePath = "Shop Items";
    [SerializeField] private int shopItemsPerRound = 3;
    [SerializeField] private MutationData mutationData;
    [SerializeField] private RoundConfigData roundConfig;

    public event Action<IReadOnlyList<ShopItemData>> OnShopCatalogReady;

    public event Action OnModifiersChanged;

    public event Action<GameObject> OnMirrorUpgrade;

    private readonly List<ShopItemData> allShopItems = new List<ShopItemData>();
    private readonly UpgradeState state = new UpgradeState();
    private readonly PurchaseLedger ledger = new PurchaseLedger();
    private readonly System.Random rng = new System.Random();

    public UpgradeState State => state;

    public MutationData MutationData => mutationData;

    protected override void Awake()
    {
        base.Awake();

        if (roundConfig == null)
        {
            Debug.LogError("UpgradeManager.roundConfig is not assigned.", this);
        }

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
        state.Reset();
    }

    private void HandlePhaseChanged(GamePhase phase)
    {
        if (phase == GamePhase.Shop)
        {
            OnShopCatalogReady?.Invoke(PickRandomItems());
        }
    }

    public int RerollCost => roundConfig != null ? roundConfig.RerollCost : 0;

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

        item.Effect.Apply(state);

        GameObject mirrorPrefab = state.ConsumePendingMirrorPrefab();
        if (mirrorPrefab != null)
        {
            OnMirrorUpgrade?.Invoke(mirrorPrefab);
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
        return state.Modifiers.GetAdditive(stat);
    }

    public float GetMultiplier(ShopStatType stat)
    {
        return state.Modifiers.GetMultiplier(stat);
    }

    public float ApplyModifiers(ShopStatType stat, float baseValue)
    {
        return state.Modifiers.Apply(stat, baseValue);
    }
}

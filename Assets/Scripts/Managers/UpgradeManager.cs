using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [SerializeField] private string shopItemsResourcePath = "Shop Items";
    [SerializeField] private int shopItemsPerRound = 3;
    [SerializeField] private bool enterKeyLoadsShopForTest = true;

    public event Action<IReadOnlyList<ShopItemData>> OnShopCatalogReady;

    private readonly List<ShopItemData> allShopItems = new List<ShopItemData>();
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

        allShopItems.AddRange(Resources.LoadAll<ShopItemData>(shopItemsResourcePath));
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

    private void Update()
    {
        if (!enterKeyLoadsShopForTest || Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            OnShopCatalogReady?.Invoke(PickRandomItems());
        }
    }

    private void HandlePhaseChanged(GamePhase phase)
    {
        if (phase == GamePhase.Shop)
        {
            OnShopCatalogReady?.Invoke(PickRandomItems());
        }
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
        if (!EconomyManager.Instance.TrySpendGold(item.Cost))
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

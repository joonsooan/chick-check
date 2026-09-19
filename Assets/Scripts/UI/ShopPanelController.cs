using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanelController : MonoBehaviour
{
    [SerializeField] private Transform itemContainer;
    [SerializeField] private ShopItemWidget itemWidgetPrefab;
    [SerializeField] private Button nextRoundButton;
    [SerializeField] private Button shopTabButton;
    [SerializeField] private Button checkTabButton;
    [SerializeField] private Button rerollButton;
    [SerializeField] private TMP_Text rerollCostText;
    [SerializeField] private Color normalCostColor = Color.white;
    [SerializeField] private Color insufficientCostColor = Color.red;

    private readonly List<ShopItemWidget> spawnedWidgets = new List<ShopItemWidget>();

    private void Awake()
    {
        if (nextRoundButton != null)
        {
            nextRoundButton.onClick.AddListener(HandleNextRoundClicked);
        }

        if (shopTabButton != null)
        {
            shopTabButton.onClick.AddListener(HandleShopTabClicked);
        }

        if (checkTabButton != null)
        {
            checkTabButton.onClick.AddListener(HandleCheckTabClicked);
        }

        if (rerollButton != null)
        {
            rerollButton.onClick.AddListener(HandleRerollClicked);
        }
    }

    private void HandleRerollClicked()
    {
        if (UpgradeManager.Instance != null && UpgradeManager.Instance.TryReroll())
        {
            UpdateRerollCostColor();
        }
    }

    private void Start()
    {
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnShopCatalogReady += HandleShopCatalogReady;
        }
    }

    private void OnEnable()
    {
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnGoldChanged += HandleGoldChanged;
        }

        UpdateRerollCostColor();
    }

    private void OnDisable()
    {
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnGoldChanged -= HandleGoldChanged;
        }
    }

    private void OnDestroy()
    {
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnShopCatalogReady -= HandleShopCatalogReady;
        }
    }

    private void HandleGoldChanged(int gold)
    {
        UpdateRerollCostColor(gold);
    }

    private void UpdateRerollCostColor()
    {
        if (EconomyManager.Instance != null)
        {
            UpdateRerollCostColor(EconomyManager.Instance.Gold);
        }
    }

    private void UpdateRerollCostColor(int gold)
    {
        if (rerollCostText == null || UpgradeManager.Instance == null)
        {
            return;
        }

        rerollCostText.color = gold < UpgradeManager.Instance.RerollCost ? insufficientCostColor : normalCostColor;
    }

    private void HandleShopTabClicked()
    {
        UIManager.Instance.ShowShop();
    }

    private void HandleCheckTabClicked()
    {
        UIManager.Instance.ShowCheck();
    }

    private void HandleShopCatalogReady(IReadOnlyList<ShopItemData> catalog)
    {
        foreach (ShopItemWidget widget in spawnedWidgets)
        {
            Destroy(widget.gameObject);
        }
        spawnedWidgets.Clear();

        for (int i = 0; i < catalog.Count; i++)
        {
            ShopItemWidget widget = Instantiate(itemWidgetPrefab, itemContainer);
            widget.Setup(catalog[i]);
            spawnedWidgets.Add(widget);
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)itemContainer);
    }

    private void HandleNextRoundClicked()
    {
        GameManager.Instance.EndShopPhase();
    }
}

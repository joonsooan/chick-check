using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanelController : MonoBehaviour
{
    [SerializeField] private Transform itemContainer;
    [SerializeField] private ShopItemWidget itemWidgetPrefab;
    [SerializeField] private Button nextRoundButton;
    [SerializeField] private Button shopTabButton;
    [SerializeField] private Button checkTabButton;

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
    }

    private void Start()
    {
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnShopCatalogReady += HandleShopCatalogReady;
        }
    }

    private void OnDestroy()
    {
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnShopCatalogReady -= HandleShopCatalogReady;
        }
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

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)itemContainer);
    }

    private void HandleNextRoundClicked()
    {
        GameManager.Instance.EndShopPhase();
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanelController : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Transform itemContainer;
    [SerializeField] private ShopItemWidget itemWidgetPrefab;
    [SerializeField] private Button nextRoundButton;

    private readonly List<ShopItemWidget> spawnedWidgets = new List<ShopItemWidget>();

    private void Awake()
    {
        if (nextRoundButton != null)
        {
            nextRoundButton.onClick.AddListener(HandleNextRoundClicked);
        }
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPhaseChanged += HandlePhaseChanged;
        }

        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnShopCatalogReady += HandleShopCatalogReady;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPhaseChanged -= HandlePhaseChanged;
        }

        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnShopCatalogReady -= HandleShopCatalogReady;
        }
    }

    private void HandlePhaseChanged(GamePhase phase)
    {
        panelRoot.SetActive(phase == GamePhase.Shop);
    }

    private void HandleShopCatalogReady(IReadOnlyList<ShopItemData> catalog)
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

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

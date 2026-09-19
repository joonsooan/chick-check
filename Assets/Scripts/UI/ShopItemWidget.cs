using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemWidget : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Button buyButton;
    [SerializeField] private Color normalCostColor = Color.white;
    [SerializeField] private Color insufficientCostColor = Color.red;

    private RectTransform rectTransform;
    private ShopItemData boundItem;

    private void Awake()
    {
        rectTransform = (RectTransform)transform;
    }

    private void OnEnable()
    {
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnGoldChanged += HandleGoldChanged;
            UpdateCostColor(EconomyManager.Instance.Gold);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    private void OnDisable()
    {
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnGoldChanged -= HandleGoldChanged;
        }
    }

    public void Setup(ShopItemData item)
    {
        if (rectTransform == null)
        {
            rectTransform = (RectTransform)transform;
        }

        boundItem = item;

        if (icon != null)
        {
            icon.sprite = item.Icon;
            icon.SetNativeSize();
        }
        nameText.text = item.ItemName;
        descriptionText.text = item.Description;
        RefreshCostText();

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(HandleBuyClicked);

        if (EconomyManager.Instance != null)
        {
            UpdateCostColor(EconomyManager.Instance.Gold);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    private void HandleGoldChanged(int gold)
    {
        UpdateCostColor(gold);
    }

    private void RefreshCostText()
    {
        if (costText == null || boundItem == null || UpgradeManager.Instance == null)
        {
            return;
        }

        costText.text = UpgradeManager.Instance.GetCurrentCost(boundItem).ToString();
    }

    private void UpdateCostColor(int gold)
    {
        if (costText == null || boundItem == null || UpgradeManager.Instance == null)
        {
            return;
        }

        costText.color = gold < UpgradeManager.Instance.GetCurrentCost(boundItem) ? insufficientCostColor : normalCostColor;
    }

    private void HandleBuyClicked()
    {
        if (UpgradeManager.Instance.TryPurchase(boundItem))
        {
            RefreshCostText();
            if (EconomyManager.Instance != null)
            {
                UpdateCostColor(EconomyManager.Instance.Gold);
            }
        }
    }
}

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

    private ShopItemData boundItem;

    private void Start()
    {
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnGoldChanged += HandleGoldChanged;
        }
    }

    private void OnDestroy()
    {
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnGoldChanged -= HandleGoldChanged;
        }
    }

    public void Setup(ShopItemData item)
    {
        boundItem = item;

        if (icon != null)
        {
            icon.sprite = item.Icon;
            icon.SetNativeSize();
        }
        nameText.text = item.ItemName;
        descriptionText.text = item.Description;
        if (costText != null)
        {
            costText.text = item.Cost.ToString();
        }

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(HandleBuyClicked);

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)buyButton.transform);

        if (EconomyManager.Instance != null)
        {
            UpdateCostColor(EconomyManager.Instance.Gold);
        }
    }

    private void HandleGoldChanged(int gold)
    {
        UpdateCostColor(gold);
    }

    private void UpdateCostColor(int gold)
    {
        if (costText == null || boundItem == null)
        {
            return;
        }

        costText.color = gold < boundItem.Cost ? insufficientCostColor : normalCostColor;
    }

    private void HandleBuyClicked()
    {
        UpgradeManager.Instance.TryPurchase(boundItem);
    }
}

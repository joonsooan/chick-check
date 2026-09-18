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

    private ShopItemData boundItem;

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
    }

    private void HandleBuyClicked()
    {
        UpgradeManager.Instance.TryPurchase(boundItem);
    }
}

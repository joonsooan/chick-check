using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemWidget : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private Button buyButton;

    [SerializeField] private string purchaseSuccessMessage = "구매 완료";
    [SerializeField] private string purchaseFailedMessage = "구매 실패";

    private ShopItemData boundItem;

    public void Setup(ShopItemData item)
    {
        boundItem = item;

        if (icon != null)
        {
            icon.sprite = item.Icon;
        }
        nameText.text = item.ItemName;
        descriptionText.text = item.Description;
        costText.text = item.Cost.ToString();
        if (feedbackText != null)
        {
            feedbackText.text = string.Empty;
        }

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(HandleBuyClicked);
    }

    private void OnEnable()
    {
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnItemPurchased += HandleItemPurchased;
            UpgradeManager.Instance.OnPurchaseFailed += HandlePurchaseFailed;
        }
    }

    private void OnDisable()
    {
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnItemPurchased -= HandleItemPurchased;
            UpgradeManager.Instance.OnPurchaseFailed -= HandlePurchaseFailed;
        }
    }

    private void HandleBuyClicked()
    {
        UpgradeManager.Instance.TryPurchase(boundItem);
    }

    private void HandleItemPurchased(ShopItemData item)
    {
        if (item != boundItem || feedbackText == null)
        {
            return;
        }
        feedbackText.color = Color.green;
        feedbackText.text = purchaseSuccessMessage;
    }

    private void HandlePurchaseFailed(ShopItemData item, string reason)
    {
        if (item != boundItem || feedbackText == null)
        {
            return;
        }
        feedbackText.color = Color.red;
        feedbackText.text = purchaseFailedMessage;
    }
}

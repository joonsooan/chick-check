using DG.Tweening;
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
    [SerializeField] private GameObject soldImage;
    [SerializeField] private TMP_Text soldText;
    [SerializeField] private Color normalCostColor = Color.white;
    [SerializeField] private Color insufficientCostColor = Color.red;

    [Header("Sold VFX")]
    [SerializeField, Min(0)] private float soldPunchDuration = 0.35f;
    [SerializeField] private float soldPunchAngle = 20f;
    [SerializeField, Min(0)] private int soldPunchVibrato = 12;
    [SerializeField, Range(0f, 1f)] private float soldPunchElasticity = 0.6f;
    [SerializeField] private Vector3 soldPunchScale = new Vector3(0.3f, 0.3f, 0f);

    private ShopItemData boundItem;

    private void OnEnable()
    {
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnGoldChanged += HandleGoldChanged;
            UpdateCostColor(EconomyManager.Instance.Gold);
        }
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
        buyButton.interactable = true;

        if (soldImage != null)
        {
            soldImage.SetActive(false);
        }

        if (soldText != null)
        {
            soldText.gameObject.SetActive(false);
        }

        if (EconomyManager.Instance != null)
        {
            UpdateCostColor(EconomyManager.Instance.Gold);
        }
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

            MarkAsSold();
            AudioManager.Instance?.PlayShopBuy();
        }
    }

    private void MarkAsSold()
    {
        if (buyButton != null)
        {
            buyButton.interactable = false;
        }

        if (soldImage != null)
        {
            soldImage.SetActive(true);
        }

        if (soldText != null)
        {
            soldText.gameObject.SetActive(true);

            RectTransform soldRect = soldText.rectTransform;
            soldRect.DOKill();
            soldRect.localScale = Vector3.one;
            soldRect.localRotation = Quaternion.identity;
            soldRect.DOPunchRotation(new Vector3(0f, 0f, soldPunchAngle), soldPunchDuration,
                soldPunchVibrato, soldPunchElasticity).SetUpdate(true);
            soldRect.DOPunchScale(soldPunchScale, soldPunchDuration).SetUpdate(true);
        }
    }
}

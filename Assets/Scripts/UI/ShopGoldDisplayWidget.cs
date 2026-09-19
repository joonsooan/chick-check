using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopGoldDisplayWidget : MonoBehaviour
{
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private GameObject displayPanel;

    private void Start()
    {
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnGoldChanged += HandleGoldChanged;
            HandleGoldChanged(EconomyManager.Instance.Gold);
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.OnShopUiVisibilityChanged += HandleShopUiVisibilityChanged;
        }
    }

    private void OnDisable()
    {
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnGoldChanged -= HandleGoldChanged;
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.OnShopUiVisibilityChanged -= HandleShopUiVisibilityChanged;
        }
    }

    private void HandleGoldChanged(int gold)
    {
        if (goldText != null)
        {
            goldText.text = gold.ToString();
        }
    }

    private void HandleShopUiVisibilityChanged(bool isVisible)
    {
        if (displayPanel == null)
        {
            return;
        }

        displayPanel.SetActive(isVisible);

        if (isVisible)
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)displayPanel.transform);
        }
    }
}

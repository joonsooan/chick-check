using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultEntityWidget : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text numText;

    public void Setup(Sprite sprite, int totalPrice, int count, bool showCount)
    {
        if (icon != null)
        {
            icon.sprite = sprite;
            icon.SetNativeSize();
        }

        if (priceText != null)
        {
            priceText.text = $"{totalPrice}";
        }

        if (numText != null)
        {
            numText.text = $"x {count}";
            numText.gameObject.SetActive(showCount);
        }
    }
}

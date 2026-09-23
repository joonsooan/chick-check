using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultEntityWidget : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text numText;
    [SerializeField, Min(0)] private float countUpDuration = 0.4f;

    private Tween countUpTween;

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
            numText.gameObject.SetActive(showCount);

            if (showCount)
            {
                PlayCountUp(count);
            }
        }
    }

    private void PlayCountUp(int count)
    {
        countUpTween?.Kill();

        if (countUpDuration <= 0f || count <= 0)
        {
            numText.text = $"x {count}";
            return;
        }

        int lastShown = -1;
        numText.text = "x 0";
        countUpTween = DOTween.To(() => 0f, value =>
        {
            int shown = Mathf.RoundToInt(value);
            if (shown == lastShown)
            {
                return;
            }

            lastShown = shown;
            numText.text = $"x {shown}";
        }, count, countUpDuration).SetEase(Ease.OutCubic).SetUpdate(true);
    }

    private void OnDisable()
    {
        countUpTween?.Kill();
        countUpTween = null;
    }
}

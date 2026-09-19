using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private RectTransform infoPanel;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundStarted += HandleRoundStarted;
            GameManager.Instance.OnPhaseTimerTick += HandlePhaseTimerTick;
            GameManager.Instance.OnRoundEnded += HandleRoundEnded;

            HandleRoundStarted(GameManager.Instance.CurrentRound);
        }

        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnGoldChanged += HandleGoldChanged;
            HandleGoldChanged(EconomyManager.Instance.Gold);
        }

        if (infoPanel != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(infoPanel);
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundStarted -= HandleRoundStarted;
            GameManager.Instance.OnPhaseTimerTick -= HandlePhaseTimerTick;
            GameManager.Instance.OnRoundEnded -= HandleRoundEnded;
        }

        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnGoldChanged -= HandleGoldChanged;
        }
    }

    private void HandleRoundStarted(int roundNumber)
    {
        roundText.text = $"Day {roundNumber}";

        if (infoPanel != null)
        {
            infoPanel.gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(infoPanel);
        }
    }

    private void HandleRoundEnded(int roundNumber)
    {
        if (infoPanel != null)
        {
            infoPanel.gameObject.SetActive(false);
        }
    }

    private void HandlePhaseTimerTick(float remaining, float total)
    {
        int totalSeconds = Mathf.CeilToInt(remaining);
        timerText.text = $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";
    }

    private void HandleGoldChanged(int gold)
    {
        goldText.text = gold.ToString();

        RectTransform parent = goldText.transform.parent as RectTransform;
        if (parent != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(parent);
        }
    }
}

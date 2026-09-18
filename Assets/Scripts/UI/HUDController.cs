using TMPro;
using UnityEngine;

public class HUDController : MonoBehaviour
{
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text phaseText;

    [SerializeField] private string roundFormat = "Round {0}";
    [SerializeField] private string goldFormat = "Gold {0}";
    [SerializeField] private string playingPhaseLabel = "플레이 중";
    [SerializeField] private string shopPhaseLabel = "상점";
    [SerializeField] private string debtCheckpointPhaseLabel = "빚 상환";
    [SerializeField] private string gameOverPhaseLabel = "게임 오버";

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundStarted += HandleRoundStarted;
            GameManager.Instance.OnPhaseTimerTick += HandlePhaseTimerTick;
            GameManager.Instance.OnPhaseChanged += HandlePhaseChanged;

            HandleRoundStarted(GameManager.Instance.CurrentRound);
            HandlePhaseChanged(GameManager.Instance.CurrentPhase);
        }

        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnGoldChanged += HandleGoldChanged;
            HandleGoldChanged(EconomyManager.Instance.Gold);
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundStarted -= HandleRoundStarted;
            GameManager.Instance.OnPhaseTimerTick -= HandlePhaseTimerTick;
            GameManager.Instance.OnPhaseChanged -= HandlePhaseChanged;
        }

        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnGoldChanged -= HandleGoldChanged;
        }
    }

    private void HandleRoundStarted(int roundNumber)
    {
        roundText.text = string.Format(roundFormat, roundNumber);
    }

    private void HandlePhaseTimerTick(float remaining, float total)
    {
        timerText.text = Mathf.CeilToInt(remaining).ToString();
    }

    private void HandleGoldChanged(int gold)
    {
        goldText.text = string.Format(goldFormat, gold);
    }

    private void HandlePhaseChanged(GamePhase phase)
    {
        switch (phase)
        {
            case GamePhase.Playing:
                phaseText.text = playingPhaseLabel;
                break;
            case GamePhase.Shop:
                phaseText.text = shopPhaseLabel;
                break;
            case GamePhase.DebtCheckpoint:
                phaseText.text = debtCheckpointPhaseLabel;
                break;
            case GamePhase.GameOver:
                phaseText.text = gameOverPhaseLabel;
                break;
        }
    }
}

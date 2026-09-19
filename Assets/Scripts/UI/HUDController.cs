using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text eggCountText;
    [SerializeField] private TMP_Text chickCountText;
    [SerializeField] private TMP_Text chickenCountText;
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

        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.OnEggCountChanged += HandleEggCountChanged;
            SpawnManager.Instance.OnChickCountChanged += HandleChickCountChanged;
            SpawnManager.Instance.OnChickenCountChanged += HandleChickenCountChanged;

            HandleEggCountChanged(SpawnManager.Instance.EggCount);
            HandleChickCountChanged(SpawnManager.Instance.ChickCount);
            HandleChickenCountChanged(SpawnManager.Instance.ChickenCount);
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

        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.OnEggCountChanged -= HandleEggCountChanged;
            SpawnManager.Instance.OnChickCountChanged -= HandleChickCountChanged;
            SpawnManager.Instance.OnChickenCountChanged -= HandleChickenCountChanged;
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

    private void HandleEggCountChanged(int count)
    {
        if (eggCountText != null)
        {
            eggCountText.text = count.ToString();
        }
    }

    private void HandleChickCountChanged(int count)
    {
        if (chickCountText != null)
        {
            chickCountText.text = count.ToString();
        }
    }

    private void HandleChickenCountChanged(int count)
    {
        if (chickenCountText != null)
        {
            chickenCountText.text = count.ToString();
        }
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

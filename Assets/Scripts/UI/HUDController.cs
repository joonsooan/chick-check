using DG.Tweening;
using TMPro;
using UnityEngine;

public class HUDController : MonoBehaviour
{
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text eggCountText;
    [SerializeField] private TMP_Text chickCountText;
    [SerializeField] private TMP_Text chickenCountText;
    [SerializeField] private RectTransform infoPanel;

    [Header("Gold VFX")]
    [SerializeField] private Vector3 goldPunch = new Vector3(0.3f, 0.3f, 0f);
    [SerializeField, Min(0)] private float goldPunchDuration = 0.3f;
    [SerializeField, Min(0)] private int goldPunchVibrato = 1;
    [SerializeField, Range(0f, 1f)] private float goldPunchElasticity = 0.5f;

    private bool layoutDirty;
    private bool goldInitialized;
    private int lastDisplayedTimerSeconds = -1;

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

        layoutDirty = true;
    }

    private void LateUpdate()
    {
        if (layoutDirty)
        {
            UILayoutUtil.Rebuild(infoPanel);
            layoutDirty = false;
        }
    }

    private void OnDestroy()
    {
        if (goldText != null)
        {
            goldText.transform.DOKill();
        }

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
        if (roundText != null)
        {
            roundText.text = $"Day {roundNumber}";
        }

        if (infoPanel != null)
        {
            infoPanel.gameObject.SetActive(true);
        }

        lastDisplayedTimerSeconds = -1;
        layoutDirty = true;
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
        if (timerText == null)
        {
            return;
        }

        int totalSeconds = Mathf.CeilToInt(remaining);
        if (totalSeconds == lastDisplayedTimerSeconds)
        {
            return;
        }

        lastDisplayedTimerSeconds = totalSeconds;
        timerText.text = $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";
    }

    private void HandleEggCountChanged(int count)
    {
        if (eggCountText != null)
        {
            eggCountText.text = count.ToString();
        }

        layoutDirty = true;
    }

    private void HandleChickCountChanged(int count)
    {
        if (chickCountText != null)
        {
            chickCountText.text = count.ToString();
        }

        layoutDirty = true;
    }

    private void HandleChickenCountChanged(int count)
    {
        if (chickenCountText != null)
        {
            chickenCountText.text = count.ToString();
        }

        layoutDirty = true;
    }

    private void HandleGoldChanged(int gold)
    {
        if (goldText != null)
        {
            goldText.text = gold.ToString();

            if (goldInitialized && goldPunchDuration > 0f)
            {
                goldText.transform.DOKill();
                goldText.transform.localScale = Vector3.one;
                goldText.transform.DOPunchScale(goldPunch, goldPunchDuration, goldPunchVibrato, goldPunchElasticity);
            }
        }

        goldInitialized = true;
        layoutDirty = true;
    }
}

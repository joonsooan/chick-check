using System.Collections.Generic;
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

    [Header("Merged Entities")]
    [SerializeField] private TMP_Text mergedEggCountText;
    [SerializeField] private TMP_Text mergedChickCountText;
    [SerializeField] private TMP_Text mergedChickenCountText;
    [SerializeField] private GameObject[] mergedEntityPanels;
    [SerializeField] private RectTransform entityPanelParent;
    [SerializeField, Min(0f)] private float expandedEntityPanelParentHeight = 224f;

    [Header("Gold VFX")]
    [SerializeField] private Vector3 goldPunch = new Vector3(0.3f, 0.3f, 0f);
    [SerializeField, Min(0)] private float goldPunchDuration = 0.3f;
    [SerializeField, Min(0)] private int goldPunchVibrato = 1;
    [SerializeField, Range(0f, 1f)] private float goldPunchElasticity = 0.5f;

    private readonly Dictionary<EntityKind, TMP_Text> countTexts = new Dictionary<EntityKind, TMP_Text>();

    private bool layoutDirty;
    private bool goldInitialized;
    private bool mergedPanelsShown;
    private int lastDisplayedTimerSeconds = -1;

    private void Awake()
    {
        countTexts[EntityKind.Egg] = eggCountText;
        countTexts[EntityKind.Chick] = chickCountText;
        countTexts[EntityKind.Chicken] = chickenCountText;
        countTexts[EntityKind.MergedEgg] = mergedEggCountText;
        countTexts[EntityKind.MergedChick] = mergedChickCountText;
        countTexts[EntityKind.MergedChicken] = mergedChickenCountText;
    }

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
            SpawnManager.Instance.OnKindCountChanged += HandleKindCountChanged;

            foreach (EntityKind kind in countTexts.Keys)
            {
                HandleKindCountChanged(kind, SpawnManager.Instance.CountOf(kind));
            }
        }

        SetMergedPanelsShown(false);

        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnModifiersChanged += HandleModifiersChanged;
            HandleModifiersChanged();
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
            SpawnManager.Instance.OnKindCountChanged -= HandleKindCountChanged;
        }

        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnModifiersChanged -= HandleModifiersChanged;
        }
    }

    private void HandleModifiersChanged()
    {
        if (mergedPanelsShown || UpgradeManager.Instance == null)
        {
            return;
        }

        if (UpgradeManager.Instance.State.Get<ChickMergeState>().HasAnyUnlocked)
        {
            SetMergedPanelsShown(true);
        }
    }

    private void SetMergedPanelsShown(bool shown)
    {
        mergedPanelsShown = shown;

        if (mergedEntityPanels != null)
        {
            for (int i = 0; i < mergedEntityPanels.Length; i++)
            {
                if (mergedEntityPanels[i] != null)
                {
                    mergedEntityPanels[i].SetActive(shown);
                }
            }
        }

        if (shown && entityPanelParent != null)
        {
            Vector2 size = entityPanelParent.sizeDelta;
            size.y = expandedEntityPanelParentHeight;
            entityPanelParent.sizeDelta = size;
        }

        layoutDirty = true;
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

    private void HandleKindCountChanged(EntityKind kind, int count)
    {
        if (!countTexts.TryGetValue(kind, out TMP_Text text))
        {
            return;
        }

        if (text != null)
        {
            text.text = count.ToString();
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

using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ResultsPanelController : MonoBehaviour
{
    [SerializeField] private Button nextButton;
    [SerializeField] private Transform entityContainer;
    [SerializeField] private ResultEntityWidget entityWidgetPrefab;
    [SerializeField] private Transform defaultWidget;
    [SerializeField] private TMP_Text totalGoldText;
    [SerializeField] private ScrollRect scrollRect;

    [Header("Reveal")]
    [SerializeField] private float initialDelay = 0.5f;
    [SerializeField] private float revealInterval = 0.35f;
    [SerializeField, Min(0)] private float totalCountUpDuration = 0.5f;

    private readonly List<ResultEntityWidget> spawnedWidgets = new List<ResultEntityWidget>();
    private List<SaleGroup> pendingGroups = new List<SaleGroup>();
    private int pendingTotalGold;
    private Coroutine revealRoutine;
    private Tween totalCountUpTween;

    private void Awake()
    {
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(HandleNextClicked);
        }
    }

    private void OnEnable()
    {
        if (EconomyManager.Instance == null)
        {
            return;
        }

        EconomyManager.Instance.OnRoundResultsReady += HandleRoundResultsReady;

        if (EconomyManager.Instance.LastRoundSaleRecords != null)
        {
            HandleRoundResultsReady(EconomyManager.Instance.LastRoundSaleRecords);
        }
    }

    private void OnDisable()
    {
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnRoundResultsReady -= HandleRoundResultsReady;
        }

        if (revealRoutine != null)
        {
            StopCoroutine(revealRoutine);
            revealRoutine = null;
        }

        totalCountUpTween?.Kill();
        totalCountUpTween = null;
    }

    private void HandleRoundResultsReady(IReadOnlyList<SaleRecord> records)
    {
        if (revealRoutine != null)
        {
            StopCoroutine(revealRoutine);
            revealRoutine = null;
        }

        pendingGroups = RoundSettlement.Summarize(records, out int totalGold);
        pendingTotalGold = totalGold;

        for (int i = 0; i < spawnedWidgets.Count; i++)
        {
            spawnedWidgets[i].gameObject.SetActive(false);
        }

        totalCountUpTween?.Kill();
        totalCountUpTween = null;
        if (totalGoldText != null)
        {
            totalGoldText.text = "0";
        }

        if (defaultWidget != null)
        {
            defaultWidget.SetAsLastSibling();
            defaultWidget.gameObject.SetActive(false);
        }

        revealRoutine = StartCoroutine(RevealRoutine());
    }

    private IEnumerator TotalCountUpRoutine(int totalGold)
    {
        if (totalGoldText == null)
        {
            yield break;
        }

        totalCountUpTween?.Kill();
        totalCountUpTween = null;

        if (totalCountUpDuration <= 0f || totalGold <= 0)
        {
            totalGoldText.text = $"{totalGold}";
            PlayTotalCountUpSfx();
            yield break;
        }

        int lastShown = -1;
        totalGoldText.text = "0";
        totalCountUpTween = DOTween.To(() => 0f, value =>
        {
            int shown = Mathf.RoundToInt(value);
            if (shown == lastShown)
            {
                return;
            }

            lastShown = shown;
            totalGoldText.text = $"{shown}";
        }, totalGold, totalCountUpDuration).SetEase(Ease.OutCubic).SetUpdate(true);

        while (totalCountUpTween != null && totalCountUpTween.IsActive() && !totalCountUpTween.IsComplete())
        {
            if (WasAdvanceRequested())
            {
                totalCountUpTween.Complete();
                break;
            }

            yield return null;
        }

        totalCountUpTween = null;
        PlayTotalCountUpSfx();
    }

    private void PlayTotalCountUpSfx()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayResultTotal();
        }
    }

    private IEnumerator RevealRoutine()
    {
        if (nextButton != null)
        {
            nextButton.interactable = false;
        }

        if (initialDelay > 0f)
        {
            yield return WaitStepOrClick(initialDelay);
        }

        for (int i = 0; i < pendingGroups.Count; i++)
        {
            RevealWidget(i, pendingGroups[i]);
            UILayoutUtil.Rebuild((RectTransform)entityContainer);
            ScrollToBottom();

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayResultReveal();
            }

            yield return WaitStepOrClick(revealInterval);
        }

        if (defaultWidget != null)
        {
            defaultWidget.gameObject.SetActive(true);
            UILayoutUtil.Rebuild((RectTransform)entityContainer);
            ScrollToBottom();

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayResultRevealFinal();
            }
        }

        yield return TotalCountUpRoutine(pendingTotalGold);

        if (nextButton != null)
        {
            nextButton.interactable = true;
        }

        revealRoutine = null;
    }

    private void RevealWidget(int index, SaleGroup group)
    {
        if (index >= spawnedWidgets.Count)
        {
            spawnedWidgets.Add(Instantiate(entityWidgetPrefab, entityContainer));
        }

        ResultEntityWidget widget = spawnedWidgets[index];
        widget.Setup(group.Sprite, group.TotalPrice, group.Count, group.ShowCount);
        widget.transform.SetSiblingIndex(index);
        widget.gameObject.SetActive(true);
    }

    private IEnumerator WaitStepOrClick(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (WasAdvanceRequested())
            {
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private static bool WasAdvanceRequested()
    {
        Mouse mouse = Mouse.current;
        return mouse != null && mouse.leftButton.wasPressedThisFrame;
    }

    private void ScrollToBottom()
    {
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    private void HandleNextClicked()
    {
        UIManager.Instance.ShowShop();
    }
}

using System.Collections;
using System.Collections.Generic;
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

    private readonly List<ResultEntityWidget> spawnedWidgets = new List<ResultEntityWidget>();
    private List<SaleGroup> pendingGroups = new List<SaleGroup>();
    private Coroutine revealRoutine;

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
    }

    private void HandleRoundResultsReady(IReadOnlyList<SaleRecord> records)
    {
        if (revealRoutine != null)
        {
            StopCoroutine(revealRoutine);
            revealRoutine = null;
        }

        pendingGroups = RoundSettlement.Summarize(records, out int totalGold);

        for (int i = 0; i < spawnedWidgets.Count; i++)
        {
            spawnedWidgets[i].gameObject.SetActive(false);
        }

        if (totalGoldText != null)
        {
            totalGoldText.text = $"{totalGold}";
        }

        if (defaultWidget != null)
        {
            defaultWidget.SetAsLastSibling();
            defaultWidget.gameObject.SetActive(false);
        }

        revealRoutine = StartCoroutine(RevealRoutine());
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

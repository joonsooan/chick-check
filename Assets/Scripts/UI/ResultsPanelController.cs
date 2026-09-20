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

        foreach (ResultEntityWidget widget in spawnedWidgets)
        {
            Destroy(widget.gameObject);
        }
        spawnedWidgets.Clear();

        List<Sprite> groupOrder = new List<Sprite>();
        Dictionary<Sprite, int> groupTotalPrice = new Dictionary<Sprite, int>();
        Dictionary<Sprite, int> groupCount = new Dictionary<Sprite, int>();
        Dictionary<Sprite, int> groupDisplayOrder = new Dictionary<Sprite, int>();

        int totalGold = 0;
        for (int i = 0; i < records.Count; i++)
        {
            Sprite sprite = records[i].Sprite;
            if (!groupTotalPrice.ContainsKey(sprite))
            {
                groupOrder.Add(sprite);
                groupTotalPrice[sprite] = 0;
                groupCount[sprite] = 0;
                groupDisplayOrder[sprite] = records[i].DisplayOrder;
            }

            groupTotalPrice[sprite] += records[i].SellPrice;
            groupCount[sprite]++;
            totalGold += records[i].SellPrice;
        }

        StableSortByDisplayOrder(groupOrder, groupDisplayOrder);

        for (int i = 0; i < groupOrder.Count; i++)
        {
            Sprite sprite = groupOrder[i];
            ResultEntityWidget widget = Instantiate(entityWidgetPrefab, entityContainer);
            widget.Setup(sprite, groupTotalPrice[sprite], groupCount[sprite]);
            widget.gameObject.SetActive(false);
            spawnedWidgets.Add(widget);
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

        for (int i = 0; i < spawnedWidgets.Count; i++)
        {
            spawnedWidgets[i].gameObject.SetActive(true);
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

    private static void StableSortByDisplayOrder(List<Sprite> order, Dictionary<Sprite, int> displayOrder)
    {
        for (int i = 1; i < order.Count; i++)
        {
            Sprite current = order[i];
            int currentKey = displayOrder[current];
            int j = i - 1;
            while (j >= 0 && displayOrder[order[j]] > currentKey)
            {
                order[j + 1] = order[j];
                j--;
            }
            order[j + 1] = current;
        }
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

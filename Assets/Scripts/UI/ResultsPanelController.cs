using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultsPanelController : MonoBehaviour
{
    [SerializeField] private Button nextButton;
    [SerializeField] private Transform entityContainer;
    [SerializeField] private ResultEntityWidget entityWidgetPrefab;
    [SerializeField] private Transform defaultWidget;
    [SerializeField] private TMP_Text totalGoldText;

    private readonly List<ResultEntityWidget> spawnedWidgets = new List<ResultEntityWidget>();

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
    }

    private void HandleRoundResultsReady(IReadOnlyList<SaleRecord> records)
    {
        foreach (ResultEntityWidget widget in spawnedWidgets)
        {
            Destroy(widget.gameObject);
        }
        spawnedWidgets.Clear();

        List<Sprite> groupOrder = new List<Sprite>();
        Dictionary<Sprite, int> groupTotalPrice = new Dictionary<Sprite, int>();
        Dictionary<Sprite, int> groupCount = new Dictionary<Sprite, int>();

        int totalGold = 0;
        for (int i = 0; i < records.Count; i++)
        {
            Sprite sprite = records[i].Sprite;
            if (!groupTotalPrice.ContainsKey(sprite))
            {
                groupOrder.Add(sprite);
                groupTotalPrice[sprite] = 0;
                groupCount[sprite] = 0;
            }

            groupTotalPrice[sprite] += records[i].SellPrice;
            groupCount[sprite]++;
            totalGold += records[i].SellPrice;
        }

        for (int i = 0; i < groupOrder.Count; i++)
        {
            Sprite sprite = groupOrder[i];
            ResultEntityWidget widget = Instantiate(entityWidgetPrefab, entityContainer);
            widget.Setup(sprite, groupTotalPrice[sprite], groupCount[sprite]);
            spawnedWidgets.Add(widget);
        }

        if (totalGoldText != null)
        {
            totalGoldText.text = $"{totalGold}";
        }

        if (defaultWidget != null)
        {
            defaultWidget.SetAsLastSibling();
        }

        UILayoutUtil.Rebuild((RectTransform)entityContainer);
    }

    private void HandleNextClicked()
    {
        UIManager.Instance.ShowShop();
    }
}

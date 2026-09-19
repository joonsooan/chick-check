using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private GameObject checkPanel;
    [SerializeField] private GameObject tabButtonGroup;
    [SerializeField] private GameObject hudPanelRoot;

    protected override void Awake()
    {
        base.Awake();
        HideAll();
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundEnded += HandleRoundEnded;
            GameManager.Instance.OnRoundStarted += HandleRoundStarted;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundEnded -= HandleRoundEnded;
            GameManager.Instance.OnRoundStarted -= HandleRoundStarted;
        }
    }

    private void HandleRoundEnded(int roundNumber)
    {
        ShowResults();
    }

    private void HandleRoundStarted(int roundNumber)
    {
        HideAll();
    }

    public void ShowShop()
    {
        SetActivePanel(shopPanel);
    }

    public void ShowResults()
    {
        SetActivePanel(resultsPanel);
    }

    public void ShowCheck()
    {
        SetActivePanel(checkPanel);
    }

    private void SetActivePanel(GameObject target)
    {
        hudPanelRoot.SetActive(true);

        bool showResults = target == resultsPanel;

        shopPanel.SetActive(showResults || target == shopPanel);
        resultsPanel.SetActive(showResults);
        checkPanel.SetActive(target == checkPanel);
        tabButtonGroup.SetActive(target == shopPanel || target == checkPanel);

        if (showResults)
        {
            resultsPanel.transform.SetAsLastSibling();
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)target.transform);
    }

    private void HideAll()
    {
        shopPanel.SetActive(false);
        resultsPanel.SetActive(false);
        checkPanel.SetActive(false);
        tabButtonGroup.SetActive(false);
        hudPanelRoot.SetActive(false);
    }
}

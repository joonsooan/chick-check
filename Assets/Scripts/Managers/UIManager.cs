using System;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private GameObject checkPanel;
    [SerializeField] private GameObject tabButtonGroup;
    [SerializeField] private GameObject shopTabButton;
    [SerializeField] private GameObject hudPanelRoot;

    public event Action<bool> OnShopUiVisibilityChanged;

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

        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnDebtStatusChanged += HandleDebtStatusChanged;
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

        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnDebtStatusChanged -= HandleDebtStatusChanged;
        }
    }

    private void HandleDebtStatusChanged(int remainingDebt, int roundsUntilDue)
    {
        RefreshShopTabButton();
    }

    private bool IsShopLocked()
    {
        return EconomyManager.Instance != null && EconomyManager.Instance.IsDebtDueNow;
    }

    private void RefreshShopTabButton()
    {
        if (shopTabButton != null)
        {
            shopTabButton.SetActive(!IsShopLocked());
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
        SetActivePanel(IsShopLocked() ? checkPanel : shopPanel);
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
        RefreshShopTabButton();

        if (showResults)
        {
            resultsPanel.transform.SetAsLastSibling();
        }

        UILayoutUtil.Rebuild((RectTransform)target.transform);

        OnShopUiVisibilityChanged?.Invoke(target == shopPanel || target == checkPanel);
    }

    private void HideAll()
    {
        shopPanel.SetActive(false);
        resultsPanel.SetActive(false);
        checkPanel.SetActive(false);
        tabButtonGroup.SetActive(false);
        hudPanelRoot.SetActive(false);

        OnShopUiVisibilityChanged?.Invoke(false);
    }
}

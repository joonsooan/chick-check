using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebtPanelController : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private Button payButton;

    private void Awake()
    {
        if (payButton != null)
        {
            payButton.onClick.AddListener(HandlePayClicked);
        }
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPhaseChanged += HandlePhaseChanged;
            GameManager.Instance.OnDebtDue += HandleDebtDue;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPhaseChanged -= HandlePhaseChanged;
            GameManager.Instance.OnDebtDue -= HandleDebtDue;
        }
    }

    private void HandlePhaseChanged(GamePhase phase)
    {
        panelRoot.SetActive(phase == GamePhase.DebtCheckpoint);
    }

    private void HandleDebtDue(int amount)
    {
        amountText.text = amount.ToString();
    }

    private void HandlePayClicked()
    {
        GameManager.Instance.PayDebt();
    }
}

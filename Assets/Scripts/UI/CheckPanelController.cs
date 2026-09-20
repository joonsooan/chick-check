using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CheckPanelController : MonoBehaviour
{
    private const int DigitCount = 5;

    [SerializeField] private RectTransform panelRoot;
    [SerializeField] private TMP_Text dueInText;
    [SerializeField] private TMP_Text remainingDebtText;
    [SerializeField] private TMP_Text[] digitTexts = new TMP_Text[DigitCount];
    [SerializeField] private Button payButton;
    [SerializeField] private Button nextDayButton;
    [SerializeField] private ConfirmDialogController nextDayConfirmDialog;
    [SerializeField] private RectTransform[] digitAnchors = new RectTransform[DigitCount];
    [SerializeField] private RectTransform digitSelectButtonPanel;
    [SerializeField] private Button digitSelectButtonUp;
    [SerializeField] private Button digitSelectButtonDown;

    public event Action<int> OnSelectedDigitChanged;

    private readonly int[] digits = new int[DigitCount];
    private int selectedDigitIndex;

    private void Awake()
    {
        if (payButton != null)
        {
            payButton.onClick.AddListener(HandlePayClicked);
        }

        if (nextDayButton != null)
        {
            nextDayButton.onClick.AddListener(HandleNextDayClicked);
        }

        if (digitSelectButtonUp != null)
        {
            digitSelectButtonUp.onClick.AddListener(() => AdjustSelectedDigit(1));
        }

        if (digitSelectButtonDown != null)
        {
            digitSelectButtonDown.onClick.AddListener(() => AdjustSelectedDigit(-1));
        }
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDebtStatusChanged += HandleDebtStatusChanged;
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.OnShopUiVisibilityChanged += HandleShopUiVisibilityChanged;
        }

        ResetInput();
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDebtStatusChanged -= HandleDebtStatusChanged;
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.OnShopUiVisibilityChanged -= HandleShopUiVisibilityChanged;
        }
    }

    private void HandleShopUiVisibilityChanged(bool isVisible)
    {
        if (!isVisible)
        {
            return;
        }

        RebuildPanelLayout();
    }

    private void RebuildPanelLayout()
    {
        UILayoutUtil.Rebuild(panelRoot);
    }

    private void Update()
    {
        if (digitSelectButtonPanel == null || !digitSelectButtonPanel.gameObject.activeInHierarchy)
        {
            return;
        }

        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            MoveSelection(-1);
        }
        else if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            MoveSelection(1);
        }

        if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            AdjustSelectedDigit(1);
        }
        else if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            AdjustSelectedDigit(-1);
        }
    }

    private void MoveSelection(int direction)
    {
        selectedDigitIndex = (selectedDigitIndex + direction + DigitCount) % DigitCount;
        AudioManager.Instance?.PlayDigitMove();
        OnSelectedDigitChanged?.Invoke(selectedDigitIndex);
        RefreshDigitDisplay();
        UpdateDigitSelectButtonPanelPosition();
    }

    private void UpdateDigitSelectButtonPanelPosition()
    {
        if (digitSelectButtonPanel == null || digitAnchors == null
            || selectedDigitIndex >= digitAnchors.Length || digitAnchors[selectedDigitIndex] == null)
        {
            return;
        }

        Vector3 position = digitSelectButtonPanel.position;
        position.x = digitAnchors[selectedDigitIndex].position.x;
        digitSelectButtonPanel.position = position;
    }

    private void AdjustSelectedDigit(int direction)
    {
        digits[selectedDigitIndex] = (digits[selectedDigitIndex] + direction + 10) % 10;
        AudioManager.Instance?.PlayDigitMove();
        ClampToMaxPayable();
        RefreshDigitDisplay();
    }

    private int GetEnteredAmount()
    {
        int amount = 0;
        for (int i = 0; i < DigitCount; i++)
        {
            amount = amount * 10 + digits[i];
        }

        return amount;
    }

    private void SetEnteredAmount(int amount)
    {
        for (int i = DigitCount - 1; i >= 0; i--)
        {
            digits[i] = amount % 10;
            amount /= 10;
        }
    }

    private int GetMaxPayable()
    {
        if (GameManager.Instance == null || EconomyManager.Instance == null)
        {
            return 0;
        }

        return Mathf.Max(0, Mathf.Min(GameManager.Instance.RemainingDebt, EconomyManager.Instance.Gold));
    }

    private void ClampToMaxPayable()
    {
        int max = GetMaxPayable();
        if (GetEnteredAmount() > max)
        {
            SetEnteredAmount(max);
        }
    }

    private void ResetInput()
    {
        selectedDigitIndex = DigitCount - 1;
        SetEnteredAmount(0);
        OnSelectedDigitChanged?.Invoke(selectedDigitIndex);
        RefreshDigitDisplay();
        UpdateDigitSelectButtonPanelPosition();
    }

    private void RefreshDigitDisplay()
    {
        if (digitTexts == null)
        {
            return;
        }

        for (int i = 0; i < DigitCount && i < digitTexts.Length; i++)
        {
            if (digitTexts[i] != null)
            {
                digitTexts[i].text = digits[i].ToString();
            }
        }

        RebuildPanelLayout();
    }

    private void HandleDebtStatusChanged(int remainingDebt, int roundsUntilDue)
    {
        if (dueInText != null)
        {
            dueInText.text = $"상환까지 D - {roundsUntilDue}";
        }

        if (remainingDebtText != null)
        {
            remainingDebtText.text = $"남은 빚 : {remainingDebt}";
        }

        ClampToMaxPayable();
        RefreshDigitDisplay();
    }

    private void HandlePayClicked()
    {
        int amount = GetEnteredAmount();
        if (amount <= 0)
        {
            return;
        }

        GameManager.Instance.PayDebt(amount);
        AudioManager.Instance?.PlayPay();
        ResetInput();
    }

    private void HandleNextDayClicked()
    {
        if (nextDayConfirmDialog != null)
        {
            nextDayConfirmDialog.Show(() => GameManager.Instance.EndShopPhase());
            return;
        }

        GameManager.Instance.EndShopPhase();
    }
}

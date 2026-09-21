using System;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmDialogController : MonoBehaviour
{
    [SerializeField] private GameObject dialogRoot;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private Action pendingConfirm;

    private void Awake()
    {
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(HandleConfirmClicked);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(Hide);
        }

        Hide();
    }

    private void OnDisable()
    {
        Hide();
    }

    public void Show(Action onConfirm)
    {
        pendingConfirm = onConfirm;

        if (dialogRoot != null)
        {
            dialogRoot.SetActive(true);
        }
    }

    public void Hide()
    {
        pendingConfirm = null;

        if (dialogRoot != null)
        {
            dialogRoot.SetActive(false);
        }
    }

    private void HandleConfirmClicked()
    {
        Action confirm = pendingConfirm;

        Hide();

        confirm?.Invoke();
    }
}

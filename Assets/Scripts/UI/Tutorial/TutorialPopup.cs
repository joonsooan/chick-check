using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class TutorialPopup : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private GameObject buttonRoot;
    [SerializeField] private Button confirmButton;

    public event Action OnConfirm;

    private LocalizedString currentMessage;

    private void Awake()
    {
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(() => OnConfirm?.Invoke());
        }

        Hide();
    }

    private void OnDestroy()
    {
        Unbind();
    }

    public void Show(LocalizedString message, bool showButton)
    {
        if (root != null)
        {
            root.SetActive(true);
        }

        Unbind();
        currentMessage = message;
        if (currentMessage != null && messageText != null)
        {
            currentMessage.StringChanged += HandleMessageChanged;
        }

        if (buttonRoot != null)
        {
            buttonRoot.SetActive(showButton);
        }
    }

    public void Hide()
    {
        Unbind();
        if (root != null)
        {
            root.SetActive(false);
        }
    }

    private void HandleMessageChanged(string value)
    {
        messageText.text = value;
    }

    private void Unbind()
    {
        if (currentMessage != null)
        {
            currentMessage.StringChanged -= HandleMessageChanged;
            currentMessage = null;
        }
    }
}

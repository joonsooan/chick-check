using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPopup : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private GameObject buttonRoot;
    [SerializeField] private Button confirmButton;

    public event Action OnConfirm;

    private void Awake()
    {
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(() => OnConfirm?.Invoke());
        }

        Hide();
    }

    public void Show(string message, bool showButton)
    {
        if (root != null)
        {
            root.SetActive(true);
        }

        if (messageText != null)
        {
            messageText.text = message;
        }

        if (buttonRoot != null)
        {
            buttonRoot.SetActive(showButton);
        }
    }

    public void Hide()
    {
        if (root != null)
        {
            root.SetActive(false);
        }
    }
}

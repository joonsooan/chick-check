using UnityEngine;
using UnityEngine.UI;

public class SettingsTabs : MonoBehaviour
{
    [SerializeField] private Button soundTabButton;
    [SerializeField] private Button languageTabButton;
    [SerializeField] private GameObject soundTab;
    [SerializeField] private GameObject languageTab;

    private void Awake()
    {
        soundTabButton.onClick.AddListener(ShowSoundTab);
        languageTabButton.onClick.AddListener(ShowLanguageTab);
    }

    private void OnDestroy()
    {
        soundTabButton.onClick.RemoveListener(ShowSoundTab);
        languageTabButton.onClick.RemoveListener(ShowLanguageTab);
    }

    private void OnEnable()
    {
        ShowSoundTab();
    }

    private void ShowSoundTab()
    {
        Show(true);
    }

    private void ShowLanguageTab()
    {
        Show(false);
    }

    private void Show(bool sound)
    {
        soundTab.SetActive(sound);
        languageTab.SetActive(!sound);
        soundTabButton.interactable = !sound;
        languageTabButton.interactable = sound;
    }
}

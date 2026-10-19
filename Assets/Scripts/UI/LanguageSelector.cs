using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class LanguageSelector : MonoBehaviour
{
    [Serializable]
    private struct LanguageOption
    {
        public Button button;
        public Locale locale;
    }

    [SerializeField] private List<LanguageOption> options = new List<LanguageOption>();

    private void Awake()
    {
        foreach (LanguageOption option in options)
        {
            LocaleIdentifier id = option.locale.Identifier;
            option.button.onClick.AddListener(() => Select(id));
        }
    }

    private void OnDestroy()
    {
        foreach (LanguageOption option in options)
        {
            option.button.onClick.RemoveAllListeners();
        }
    }

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += Refresh;
        if (LocalizationSettings.InitializationOperation.IsDone)
        {
            Refresh(LocalizationSettings.SelectedLocale);
        }
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= Refresh;
    }

    private void Select(LocaleIdentifier id)
    {
        if (!LocalizationSettings.InitializationOperation.IsDone)
        {
            return;
        }

        Locale target = LocalizationSettings.AvailableLocales.GetLocale(id);
        if (target != null)
        {
            LocalizationSettings.SelectedLocale = target;
        }
    }

    private void Refresh(Locale current)
    {
        foreach (LanguageOption option in options)
        {
            option.button.interactable = current == null || option.locale.Identifier != current.Identifier;
        }
    }
}

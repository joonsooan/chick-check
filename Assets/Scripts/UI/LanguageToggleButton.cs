using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class LanguageToggleButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Locale first;
    [SerializeField] private Locale second;

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        button.onClick.AddListener(Toggle);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(Toggle);
    }

    private void Toggle()
    {
        if (!LocalizationSettings.InitializationOperation.IsDone || first == null || second == null)
        {
            return;
        }

        Locale current = LocalizationSettings.SelectedLocale;
        LocaleIdentifier targetId = current != null && current.Identifier == first.Identifier ? second.Identifier : first.Identifier;
        Locale target = LocalizationSettings.AvailableLocales.GetLocale(targetId);
        if (target != null)
        {
            LocalizationSettings.SelectedLocale = target;
        }
    }
}

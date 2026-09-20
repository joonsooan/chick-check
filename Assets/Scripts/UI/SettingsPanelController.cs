using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsPanelController : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button toTitleButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private string titleSceneName = "TitleScene";
    [SerializeField, Range(0f, 1f)] private float defaultSfxVolume = 0.5f;
    [SerializeField, Range(0f, 1f)] private float defaultBgmVolume = 0.5f;

    private void Awake()
    {

        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(Open);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Close);
        }

        if (toTitleButton != null)
        {
            toTitleButton.onClick.AddListener(GoToTitle);
            if (SceneManager.GetActiveScene().name == titleSceneName)
            {
                toTitleButton.gameObject.SetActive(false);
            }
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.AddListener(HandleSfxChanged);
        }

        if (bgmSlider != null)
        {
            bgmSlider.onValueChanged.AddListener(HandleBgmChanged);
        }

        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    private void Start()
    {
        if (sfxSlider != null)
        {
            float sfx = AudioManager.Instance != null ? AudioManager.Instance.SfxVolume : defaultSfxVolume;
            sfxSlider.SetValueWithoutNotify(sfx);
        }

        if (bgmSlider != null)
        {
            float bgm = AudioManager.Instance != null ? AudioManager.Instance.BgmVolume : defaultBgmVolume;
            bgmSlider.SetValueWithoutNotify(bgm);
        }
    }

    private void OnDestroy()
    {
        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveListener(Open);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Close);
        }

        if (toTitleButton != null)
        {
            toTitleButton.onClick.RemoveListener(GoToTitle);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(QuitGame);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(HandleSfxChanged);
        }

        if (bgmSlider != null)
        {
            bgmSlider.onValueChanged.RemoveListener(HandleBgmChanged);
        }
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        if (panelRoot == null)
        {
            return;
        }

        if (panelRoot.activeSelf)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    public void Open()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        AudioManager.Instance?.PlaySettingsToggle();
        Time.timeScale = 0f;
    }

    public void Close()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        AudioManager.Instance?.PlaySettingsToggle();
        RestorePhaseTimeScale();
    }

    private void RestorePhaseTimeScale()
    {
        bool paused = GameManager.Instance != null && GameManager.Instance.CurrentPhase != GamePhase.Playing;
        Time.timeScale = paused ? 0f : 1f;
    }

    private void GoToTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(titleSceneName);
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void HandleSfxChanged(float value)
    {
        AudioManager.Instance?.SetSfxVolume(value);
    }

    private void HandleBgmChanged(float value)
    {
        AudioManager.Instance?.SetBgmVolume(value);
    }
}

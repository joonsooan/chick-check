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
    [SerializeField] private Slider soundSlider;
    [SerializeField] private string titleSceneName = "TitleScene";

    private float soundVolume = 1f;

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

        if (soundSlider != null)
        {
            soundSlider.SetValueWithoutNotify(soundVolume);
            soundSlider.onValueChanged.AddListener(HandleSoundChanged);
        }

        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
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

        if (soundSlider != null)
        {
            soundSlider.onValueChanged.RemoveListener(HandleSoundChanged);
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
    }

    public void Close()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
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

    private void HandleSoundChanged(float value)
    {
        soundVolume = value;
    }
}

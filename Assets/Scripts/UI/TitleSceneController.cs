using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleSceneController : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button tutorialButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private GameObject startHoverImage;
    [SerializeField] private GameObject tutorialHoverImage;
    [SerializeField] private GameObject quitHoverImage;
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private string tutorialSceneName = "TutorialScene";

    private void Awake()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(StartGame);
            RegisterHover(startButton.gameObject, startHoverImage);
        }

        if (tutorialButton != null)
        {
            tutorialButton.onClick.AddListener(StartTutorial);
            RegisterHover(tutorialButton.gameObject, tutorialHoverImage);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
            RegisterHover(quitButton.gameObject, quitHoverImage);
        }

        SetHoverActive(startHoverImage, false);
        SetHoverActive(tutorialHoverImage, false);
        SetHoverActive(quitHoverImage, false);
    }

    private void RegisterHover(GameObject target, GameObject hoverImage)
    {
        if (hoverImage == null)
        {
            return;
        }

        EventTrigger trigger = target.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = target.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enter.callback.AddListener(_ => SetHoverActive(hoverImage, true));
        trigger.triggers.Add(enter);

        EventTrigger.Entry exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exit.callback.AddListener(_ => SetHoverActive(hoverImage, false));
        trigger.triggers.Add(exit);
    }

    private void SetHoverActive(GameObject hoverImage, bool active)
    {
        if (hoverImage != null)
        {
            hoverImage.SetActive(active);
        }
    }

    private void OnDestroy()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(StartGame);
        }

        if (tutorialButton != null)
        {
            tutorialButton.onClick.RemoveListener(StartTutorial);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(QuitGame);
        }
    }

    private void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    private void StartTutorial()
    {
        SceneManager.LoadScene(tutorialSceneName);
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

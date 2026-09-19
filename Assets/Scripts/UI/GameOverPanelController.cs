using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverPanelController : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button restartButton;

    private void Awake()
    {
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }

        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver += HandleGameOver;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver -= HandleGameOver;
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartGame);
        }
    }

    private void RestartGame()
    {
        Time.timeScale = 1f;
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }

    private void HandleGameOver(GameOverReason reason)
    {
        panelRoot.SetActive(true);
    }
}

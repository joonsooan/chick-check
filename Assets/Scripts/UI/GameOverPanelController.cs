using TMPro;
using UnityEngine;

public class GameOverPanelController : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text reasonText;

    [SerializeField] private string notEnoughGoldMessage = "빚을 갚지 못했습니다";
    [SerializeField] private string defaultGameOverMessage = "게임 오버";

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver += HandleGameOver;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver -= HandleGameOver;
        }
    }

    private void HandleGameOver(GameOverReason reason)
    {
        panelRoot.SetActive(true);

        switch (reason)
        {
            case GameOverReason.NotEnoughGold:
                reasonText.text = notEnoughGoldMessage;
                break;
            default:
                reasonText.text = defaultGameOverMessage;
                break;
        }
    }
}

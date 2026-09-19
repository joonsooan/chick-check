using UnityEngine;
using UnityEngine.UI;

public class ResultsPanelController : MonoBehaviour
{
    [SerializeField] private Button nextButton;

    private void Awake()
    {
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(HandleNextClicked);
        }
    }

    private void HandleNextClicked()
    {
        UIManager.Instance.ShowShop();
    }
}

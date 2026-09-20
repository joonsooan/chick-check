using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [Header("Scene refs")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerCarry playerCarry;
    [SerializeField] private Mirror mirror;
    [SerializeField] private GameObject mainCanvas;

    [Header("Tutorial UI")]
    [SerializeField] private TutorialPopup popup;

    [Header("Flow hooks")]
    [SerializeField] private Button resultsNextButton;
    [SerializeField] private Button checkButton;
    [SerializeField] private string gameSceneName = "GameScene";

    [Header("Texts")]
    [TextArea][SerializeField] private string moveText = "W, A, S, D를 눌러 움직이세요";
    [TextArea][SerializeField] private string carryText = "[Space]를 눌러 병아리를 들거나 내려놓으세요";
    [TextArea][SerializeField] private string mirrorText = "병아리를 거울 앞에 내려놓으면 2마리가 됩니다.\n닭이나 달걀은 복제할 수 없습니다.";
    [TextArea][SerializeField] private string debtText = "시간 안에 병아리를 복제해서 빚을 갚아나갑시다!";
    [TextArea][SerializeField] private string shopText = "상점에서는 다양한 아이템을 구매하실 수 있습니다.";
    [TextArea][SerializeField] private string checkText = "CHECK에서는 빚을 갚을 수 있습니다.\n상환 기간 내에 빚을 갚아나갑시다.";
    [TextArea][SerializeField] private string checkClickText = "CHECK 버튼을 눌러보세요.";
    [TextArea][SerializeField] private string endText = "튜토리얼 종료! 이제 게임을 시작해봅시다.";

    private bool duplicated;
    private bool carried;
    private bool resultsNextClicked;
    private bool confirmClicked;
    private bool checkButtonClicked;

    private void OnEnable()
    {
        if (mirror != null)
        {
            mirror.OnDuplicated += HandleDuplicated;
        }

        if (playerCarry != null)
        {
            playerCarry.OnPickedUp += HandlePickedUp;
        }

        if (popup != null)
        {
            popup.OnConfirm += HandleConfirm;
        }

        if (resultsNextButton != null)
        {
            resultsNextButton.onClick.AddListener(HandleResultsNext);
        }

        if (checkButton != null)
        {
            checkButton.onClick.AddListener(HandleCheckButton);
        }
    }

    private void OnDisable()
    {
        if (mirror != null)
        {
            mirror.OnDuplicated -= HandleDuplicated;
        }

        if (playerCarry != null)
        {
            playerCarry.OnPickedUp -= HandlePickedUp;
        }

        if (popup != null)
        {
            popup.OnConfirm -= HandleConfirm;
        }

        if (resultsNextButton != null)
        {
            resultsNextButton.onClick.RemoveListener(HandleResultsNext);
        }

        if (checkButton != null)
        {
            checkButton.onClick.RemoveListener(HandleCheckButton);
        }

        Time.timeScale = 1f;
    }

    private void HandleDuplicated() => duplicated = true;
    private void HandlePickedUp(PlaceableEntity entity) => carried = true;
    private void HandleResultsNext() => resultsNextClicked = true;
    private void HandleCheckButton() => checkButtonClicked = true;
    private void HandleConfirm()
    {
        confirmClicked = true;

        if (popup != null)
        {
            popup.Hide();
        }
    }

    private void Start()
    {
        if (mainCanvas != null)
        {
            mainCanvas.SetActive(false);
        }

        StartCoroutine(RunTutorial());
    }

    private IEnumerator RunTutorial()
    {
        yield return new WaitForSecondsRealtime(0.3f);

        // 1) 이동
        SetFreeze(true);
        popup.Show(moveText, false);
        yield return new WaitUntil(() => playerMovement != null && playerMovement.MoveInput.sqrMagnitude > 0.01f);
        SetFreeze(false);
        yield return new WaitForSecondsRealtime(0.3f);

        // 2) 병아리 들기
        carried = false;
        popup.Show(carryText, false);
        yield return new WaitUntil(() => carried);
        yield return new WaitForSecondsRealtime(0.5f);

        // 3) 거울 앞 복제
        duplicated = false;
        popup.Show(mirrorText, false);
        yield return new WaitUntil(() => duplicated);

        // 복제된 병아리들이 도망가는 연출을 잠깐 보여준 뒤 프리즈
        yield return new WaitForSecondsRealtime(1.2f);

        // 4) 타이머 / 빚 안내 + 게임 스타트
        SetFreeze(true);
        confirmClicked = false;
        popup.Show(debtText, true);
        yield return new WaitUntil(() => confirmClicked);

        // confirm 후 메인 캔버스 활성화
        if (mainCanvas != null)
        {
            mainCanvas.SetActive(true);
        }
        SetFreeze(false);

        // 타이머 시작
        if (GameManager.Instance != null)
        {
            GameManager.Instance.BeginTutorialRound();
        }

        // 5) 1라운드 종료 → 통계창 Next 대기
        resultsNextClicked = false;
        yield return new WaitUntil(() => resultsNextClicked);
        yield return null;

        // 6) 상점 안내
        yield return StepPopupWithConfirm(shopText);

        // 7) 체크 버튼 클릭 유도
        SetFreeze(true);
        checkButtonClicked = false;
        popup.Show(checkText, false);
        yield return new WaitUntil(() => checkButtonClicked);
        SetFreeze(false);

        // 8) 체크 안내
        yield return StepPopupWithConfirm(checkClickText);

        // 9) 종료 → 게임 시작 버튼
        SetFreeze(true);
        confirmClicked = false;
        popup.Show(endText, true);
        yield return new WaitUntil(() => confirmClicked);

        SetFreeze(false);
        SceneManager.LoadScene(gameSceneName);
    }

    private IEnumerator StepPopupWithConfirm(string text)
    {
        SetFreeze(true);
        confirmClicked = false;
        popup.Show(text, true);

        yield return new WaitUntil(() => confirmClicked);

        SetFreeze(false);
    }

    private void SetFreeze(bool frozen)
    {
        Time.timeScale = frozen ? 0f : 1f;
    }
}

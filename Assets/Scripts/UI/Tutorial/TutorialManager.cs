using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class TutorialManager : MonoBehaviour
{
    [Header("Scene refs")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerCarry playerCarry;
    [SerializeField] private Mirror mirror;
    [SerializeField] private GameObject mainCanvas;
    [SerializeField] private TutorialPopup popup;

    [Header("Flow")]
    [SerializeField, Min(0)] private float startDelay = 0.3f;
    [SerializeField] private List<TutorialStep> steps = new List<TutorialStep>();
    [SerializeField] private string gameSceneName = "GameScene";

    private TutorialContext context;

    private void Awake()
    {
        context = new TutorialContext(playerMovement, playerCarry, mirror, popup);
    }

    private void Start()
    {
        if (mainCanvas != null)
        {
            mainCanvas.SetActive(false);
        }

        StartCoroutine(RunTutorial());
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
    }

    private IEnumerator RunTutorial()
    {
        yield return new WaitForSecondsRealtime(startDelay);

        foreach (TutorialStep step in steps)
        {
            yield return RunStep(step);
        }

        SceneManager.LoadScene(gameSceneName);
    }

    private IEnumerator RunStep(TutorialStep step)
    {
        if (step.delayBefore > 0f)
        {
            yield return new WaitForSecondsRealtime(step.delayBefore);
        }

        TutorialCondition condition = step.condition;
        bool showConfirm = condition != null && condition.RequiresConfirmButton;

        SetFreeze(step.freezeTime);
        if (step.HasMessage)
        {
            popup.Show(step.message, showConfirm);
        }
        else
        {
            popup.Hide();
        }

        if (condition != null)
        {
            condition.Begin(context);
            yield return new WaitUntil(() => condition.IsMet(context));
            condition.End(context);
        }

        if (showConfirm)
        {
            popup.Hide();
        }

        SetFreeze(false);
        step.onCompleted?.Invoke();
    }

    private static void SetFreeze(bool frozen)
    {
        if (frozen)
        {
            Time.timeScale = 0f;
            return;
        }

        Time.timeScale = GameManager.Instance != null ? GameManager.Instance.PhaseTimeScale : 1f;
    }
}

#if UNITY_EDITOR
public partial class TutorialManager
{
    [ContextMenu("Build Default Steps")]
    private void BuildDefaultSteps()
    {
        UnityEditor.Undo.RecordObject(this, "Build Default Tutorial Steps");

        steps = new List<TutorialStep>
        {
            Step("W, A, S, D 키를 눌러 움직이세요", new MoveInputCondition(), freeze: true),
            Step("스페이스 바를 눌러 병아리를 들거나 내려놓으세요", new PickUpCondition(), delay: 0.3f),
            Step("병아리를 거울 앞에 내려놓으면 2마리가 됩니다\n닭이나 달걀은 복제할 수 없습니다", new DuplicateCondition(), delay: 0.5f),
            Step("제한 시간 안에 병아리를 최대한 많이 복제해봅시다!", new ConfirmClickCondition(), freeze: true, delay: 1.2f),
            Step(string.Empty, new ButtonClickCondition()),
            Step("상점에서는 다양한 아이템을 구매하실 수 있습니다", new ConfirmClickCondition(), freeze: true),
            Step("CHECK에서는 갚을 빚을 확인하고 빚을 갚을 수 있습니다\nCHECK 버튼을 눌러보세요", new ButtonClickCondition(), freeze: true),
            Step("A, D 키로 단위 조절,\nW, S 키로 숫자를 조절할 수 있습니다\nPay 버튼을 눌러 지불하세요", new DebtPaidCondition(), freeze: true),
            Step("튜토리얼 종료!\n이제 본격적으로 게임을 시작합시다", new ConfirmClickCondition(), freeze: true),
        };

        TutorialStep debtStep = steps[3];
        if (mainCanvas != null)
        {
            UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(debtStep.onCompleted, mainCanvas.SetActive, true);
        }

        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(debtStep.onCompleted, gameManager.BeginTutorialRound);
        }

        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log("Tutorial steps built. Assign buttons on the two ButtonClickCondition steps (results Next, CHECK).", this);
    }

    private static TutorialStep Step(string message, TutorialCondition condition, bool freeze = false, float delay = 0f)
    {
        return new TutorialStep
        {
            message = message,
            condition = condition,
            freezeTime = freeze,
            delayBefore = delay,
            onCompleted = new UnityEngine.Events.UnityEvent(),
        };
    }
}
#endif

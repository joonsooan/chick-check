using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
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
        yield return LocalizationSettings.InitializationOperation;
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
            Step("tutorial.step.move", new MoveInputCondition(), freeze: true),
            Step("tutorial.step.pickup", new PickUpCondition(), delay: 0.3f),
            Step("tutorial.step.duplicate", new DuplicateCondition(), delay: 0.5f),
            Step("tutorial.step.goal", new ConfirmClickCondition(), freeze: true, delay: 1.2f),
            Step(string.Empty, new ButtonClickCondition()),
            Step("tutorial.step.shop", new ConfirmClickCondition(), freeze: true),
            Step("tutorial.step.check", new ButtonClickCondition(), freeze: true),
            Step("tutorial.step.pay", new DebtPaidCondition(), freeze: true),
            Step("tutorial.step.end", new ConfirmClickCondition(), freeze: true),
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

    private static TutorialStep Step(string messageKey, TutorialCondition condition, bool freeze = false, float delay = 0f)
    {
        return new TutorialStep
        {
            message = string.IsNullOrEmpty(messageKey) ? new LocalizedString() : new LocalizedString("Tutorial", messageKey),
            condition = condition,
            freezeTime = freeze,
            delayBefore = delay,
            onCompleted = new UnityEngine.Events.UnityEvent(),
        };
    }
}
#endif

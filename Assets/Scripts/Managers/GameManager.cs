using System;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private RoundConfigData roundConfig;
    [SerializeField] private bool tutorialMode;

    public int CurrentRound { get; private set; }
    public GamePhase CurrentPhase { get; private set; }
    public float PhaseTimeRemaining { get; private set; }

    public event Action<int> OnRoundStarted;
    public event Action<int> OnRoundSpawnRequested;
    public event Action<float, float> OnPhaseTimerTick;
    public event Action<int> OnRoundEnding;
    public event Action<int> OnRoundEnded;
    public event Action<GamePhase> OnPhaseChanged;
    public event Action<int> OnDebtDue;
    public event Action<GameOverReason> OnGameOver;

    private float phaseTotalTime;
    private bool phaseHasTimer;

    private void Start()
    {
        if (tutorialMode)
        {
            CurrentRound = 1;
            SetPhase(GamePhase.Playing);
            OnRoundStarted?.Invoke(CurrentRound);
            phaseHasTimer = false;
            return;
        }

        StartRound(1);
    }

    public void BeginTutorialRound()
    {
        if (!tutorialMode || CurrentPhase != GamePhase.Playing)
        {
            return;
        }

        float roundDuration = roundConfig.RoundDuration;
        if (UpgradeManager.Instance != null)
        {
            roundDuration = UpgradeManager.Instance.ApplyModifiers(ShopStatType.RoundDuration, roundDuration);
        }

        SetPhaseTimer(roundDuration * roundConfig.TutorialRoundDurationRatio);
    }

    private void Update()
    {
        if (CurrentPhase == GamePhase.GameOver)
        {
            return;
        }

        if (!phaseHasTimer)
        {
            return;
        }

        PhaseTimeRemaining -= Time.deltaTime;
        OnPhaseTimerTick?.Invoke(Mathf.Max(PhaseTimeRemaining, 0f), phaseTotalTime);

        if (PhaseTimeRemaining <= 0f && CurrentPhase == GamePhase.Playing)
        {
            EndRound();
        }
    }

    public void StartRound(int roundNumber)
    {
        CurrentRound = roundNumber;
        SetPhase(GamePhase.Playing);

        OnRoundStarted?.Invoke(CurrentRound);
        OnRoundSpawnRequested?.Invoke(CurrentRound);

        float roundDuration = roundConfig.RoundDuration;
        if (UpgradeManager.Instance != null)
        {
            roundDuration = UpgradeManager.Instance.ApplyModifiers(ShopStatType.RoundDuration, roundDuration);
        }

        SetPhaseTimer(roundDuration);
    }

    public void EndRoundForDebug()
    {
        if (CurrentPhase != GamePhase.Playing)
        {
            return;
        }

        EndRound();
    }

    private void EndRound()
    {
        OnRoundEnding?.Invoke(CurrentRound);
        OnRoundEnded?.Invoke(CurrentRound);
        EnterShopPhase();
    }

    private void EnterShopPhase()
    {
        SetPhase(GamePhase.Shop);
        phaseHasTimer = false;
    }

    public void EndShopPhase()
    {
        if (CurrentPhase != GamePhase.Shop)
        {
            return;
        }

        bool debtDue = EconomyManager.Instance != null && EconomyManager.Instance.IsDebtDue(CurrentRound);
        if (debtDue)
        {
            EnterDebtCheckpoint();
            return;
        }

        StartRound(CurrentRound + 1);
    }

    private void EnterDebtCheckpoint()
    {
        SetPhase(GamePhase.DebtCheckpoint);
        phaseHasTimer = false;
        int remainingDebt = EconomyManager.Instance != null ? EconomyManager.Instance.RemainingDebt : 0;
        OnDebtDue?.Invoke(remainingDebt);
        EnterGameOver(GameOverReason.NotEnoughGold);
    }

    private void EnterGameOver(GameOverReason reason)
    {
        SetPhase(GamePhase.GameOver);
        phaseHasTimer = false;
        OnGameOver?.Invoke(reason);
    }

    private void SetPhase(GamePhase phase)
    {
        CurrentPhase = phase;
        Time.timeScale = phase == GamePhase.Playing ? 1f : 0f;
        OnPhaseChanged?.Invoke(CurrentPhase);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Time.timeScale = 1f;
    }

    private void SetPhaseTimer(float duration)
    {
        phaseTotalTime = duration;
        PhaseTimeRemaining = duration;
        phaseHasTimer = duration > 0f;
        OnPhaseTimerTick?.Invoke(Mathf.Max(PhaseTimeRemaining, 0f), phaseTotalTime);
    }
}

public enum GameOverReason
{
    NotEnoughGold
}

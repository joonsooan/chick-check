using System;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private RoundConfigData roundConfig;
    [SerializeField] private bool tutorialMode;

    private DebtLedger debtLedger;

    public int CurrentRound { get; private set; }
    public GamePhase CurrentPhase { get; private set; }
    public float PhaseTimeRemaining { get; private set; }
    public int RemainingDebt => debtLedger?.RemainingDebt ?? 0;
    public int NextDebtDueRound => debtLedger?.NextDebtDueRound ?? 0;
    public int MaxPayableDebt =>
        debtLedger != null && EconomyManager.Instance != null ? debtLedger.MaxPayable(EconomyManager.Instance.Gold) : 0;

    public event Action<int> OnRoundStarted;
    public event Action<int> OnRoundSpawnRequested;
    public event Action<float, float> OnPhaseTimerTick;
    public event Action<int> OnRoundEnding;
    public event Action<int> OnRoundEnded;
    public event Action<GamePhase> OnPhaseChanged;
    public event Action<int> OnDebtDue;
    public event Action<int, int> OnDebtStatusChanged;
    public event Action<GameOverReason> OnGameOver;

    private float phaseTotalTime;
    private bool phaseHasTimer;

    private void Start()
    {
        debtLedger = new DebtLedger(roundConfig.DebtCheckpointInterval, roundConfig.DebtAmount, roundConfig.DebtMultiplier);

        if (tutorialMode)
        {
            CurrentRound = 1;
            SetPhase(GamePhase.Playing);
            OnRoundStarted?.Invoke(CurrentRound);
            phaseHasTimer = false;
            NotifyDebtStatus();
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

        SetPhaseTimer(roundDuration * 0.5f);
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
        NotifyDebtStatus();
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

        if (debtLedger.IsDue(CurrentRound))
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
        OnDebtDue?.Invoke(RemainingDebt);
        EnterGameOver(GameOverReason.NotEnoughGold);
    }

    public void PayDebt(int amount)
    {
        if (CurrentPhase != GamePhase.Shop || EconomyManager.Instance == null)
        {
            return;
        }

        int payable = Mathf.Clamp(amount, 0, MaxPayableDebt);
        if (payable <= 0 || !EconomyManager.Instance.TrySpendGold(payable))
        {
            return;
        }

        debtLedger.Pay(payable);
        NotifyDebtStatus();
    }

    private void NotifyDebtStatus()
    {
        OnDebtStatusChanged?.Invoke(RemainingDebt, debtLedger.RoundsUntilDue(CurrentRound));
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

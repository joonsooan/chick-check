using System;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private RoundConfigData roundConfig;
    [SerializeField] private bool tutorialMode;
    [SerializeField, Min(0.1f)] private float debugPlayingTimeScale = 1f;

    public int CurrentRound { get; private set; }
    public GamePhase CurrentPhase { get; private set; }
    public float PhaseTimeRemaining { get; private set; }
    public float PhaseTimeScale => CurrentPhase == GamePhase.Playing ? debugPlayingTimeScale : 0f;

    public event Action<int> OnRoundStarted;
    public event Action<int> OnRoundSpawnRequested;
    public event Action<float, float> OnPhaseTimerTick;
    public event Action<int> OnRoundEnding;
    public event Action<int> OnRoundEnded;
    public event Action<int> OnShopPhaseEnding;
    public event Action<GamePhase> OnPhaseChanged;
    public event Action<GameOverReason> OnGameOver;

    private float phaseTotalTime;
    private bool phaseHasTimer;

    public void SetDebugPlayingTimeScale(float scale)
    {
        debugPlayingTimeScale = Mathf.Max(0.1f, scale);
        Time.timeScale = PhaseTimeScale;
    }

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

        SetPhaseTimer(roundConfig.TutorialRoundDuration);
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

        SetPhaseTimer(ModifiedRoundDuration());
    }

    private float ModifiedRoundDuration()
    {
        float roundDuration = roundConfig.RoundDuration;
        return UpgradeManager.Instance != null
            ? UpgradeManager.Instance.ApplyModifiers(ShopStatType.RoundDuration, roundDuration)
            : roundDuration;
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

        OnShopPhaseEnding?.Invoke(CurrentRound);
        if (CurrentPhase == GamePhase.GameOver)
        {
            return;
        }

        StartRound(CurrentRound + 1);
    }

    public void TriggerGameOver(GameOverReason reason)
    {
        if (CurrentPhase == GamePhase.GameOver)
        {
            return;
        }

        SetPhase(GamePhase.GameOver);
        phaseHasTimer = false;
        OnGameOver?.Invoke(reason);
    }

    private void SetPhase(GamePhase phase)
    {
        CurrentPhase = phase;
        Time.timeScale = PhaseTimeScale;
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

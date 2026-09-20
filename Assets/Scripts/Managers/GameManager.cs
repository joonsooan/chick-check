using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private RoundConfigData roundConfig;
    [SerializeField] private Vector3 chickSpawnCenter;
    [SerializeField] private bool enterKeyEndsRoundForTest = true;
    [SerializeField] private bool tutorialMode;

    public int CurrentRound { get; private set; }
    public GamePhase CurrentPhase { get; private set; }
    public float PhaseTimeRemaining { get; private set; }
    public int RemainingDebt { get; private set; }
    public int NextDebtDueRound { get; private set; }
    public int RerollCost => roundConfig.RerollCost;

    public event Action<int> OnRoundStarted;
    public event Action<float, float> OnPhaseTimerTick;
    public event Action<int> OnRoundEnding;
    public event Action<int> OnRoundEnded;
    public event Action<GamePhase> OnPhaseChanged;
    public event Action<int> OnDebtDue;
    public event Action<int, int> OnDebtStatusChanged;
    public event Action<GameOverReason> OnGameOver;

    private float phaseTotalTime;
    private bool phaseHasTimer;
    private float currentDebtAmount;

    private void Start()
    {
        NextDebtDueRound = roundConfig.DebtCheckpointInterval;
        currentDebtAmount = roundConfig.DebtAmount;
        RemainingDebt = Mathf.RoundToInt(currentDebtAmount);

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

        if (enterKeyEndsRoundForTest && CurrentPhase == GamePhase.Playing
            && Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
        {
            EndRound();
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

        int spawnCount = roundConfig.ChickSpawnCountPerRound;
        float roundDuration = roundConfig.RoundDuration;
        if (UpgradeManager.Instance != null)
        {
            spawnCount = Mathf.RoundToInt(
                UpgradeManager.Instance.ApplyModifiers(ShopStatType.ChickSpawnCountPerRound, spawnCount));
            roundDuration = UpgradeManager.Instance.ApplyModifiers(ShopStatType.RoundDuration, roundDuration);
        }

        int mutantCount = 0;
        Chick mutantPrefab = null;
        if (UpgradeManager.Instance != null && UpgradeManager.Instance.MutationData != null)
        {
            mutantPrefab = UpgradeManager.Instance.MutationData.MutatedChickPrefab;
            mutantCount = Mathf.Min(UpgradeManager.Instance.MutantSpawnCount, spawnCount);
        }

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 spawnPosition = GetRandomSpawnPoint();
            if (i < mutantCount && mutantPrefab != null)
            {
                SpawnManager.Instance.SpawnChick(mutantPrefab, spawnPosition);
            }
            else
            {
                SpawnManager.Instance.SpawnChick(spawnPosition);
            }
        }

        SetPhaseTimer(roundDuration);
        NotifyDebtStatus();
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

        if (CurrentRound >= NextDebtDueRound)
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

        int payable = Mathf.Clamp(amount, 0, Mathf.Min(RemainingDebt, EconomyManager.Instance.Gold));
        if (payable <= 0 || !EconomyManager.Instance.TrySpendGold(payable))
        {
            return;
        }

        RemainingDebt -= payable;
        if (RemainingDebt <= 0)
        {
            currentDebtAmount *= roundConfig.DebtMultiplier;
            RemainingDebt = Mathf.RoundToInt(currentDebtAmount);
            NextDebtDueRound += roundConfig.DebtCheckpointInterval;
        }

        NotifyDebtStatus();
    }

    private void NotifyDebtStatus()
    {
        int roundsUntilDue = Mathf.Max(NextDebtDueRound - CurrentRound, 0);
        OnDebtStatusChanged?.Invoke(RemainingDebt, roundsUntilDue);
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

    private Vector3 GetRandomSpawnPoint()
    {
        float outerRadius = roundConfig.ChickSpawnOuterRadius;
        float innerRadius = roundConfig.ChickSpawnInnerRadius;
        if (outerRadius <= 0f || innerRadius >= outerRadius)
        {
            return chickSpawnCenter;
        }

        float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
        float radius = Mathf.Sqrt(UnityEngine.Random.Range(innerRadius * innerRadius, outerRadius * outerRadius));
        Vector3 point = chickSpawnCenter + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
        point += (Vector3)(UnityEngine.Random.insideUnitCircle * roundConfig.ChickSpawnJitter);
        return point;
    }
}

public enum GameOverReason
{
    NotEnoughGold
}

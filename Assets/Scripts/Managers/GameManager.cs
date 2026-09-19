using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private RoundConfigData roundConfig;
    [SerializeField] private Vector3 chickSpawnCenter;
    [SerializeField] private bool enterKeyEndsRoundForTest = true;

    public int CurrentRound { get; private set; }
    public GamePhase CurrentPhase { get; private set; }
    public float PhaseTimeRemaining { get; private set; }

    public event Action<int> OnRoundStarted;
    public event Action<float, float> OnPhaseTimerTick;
    public event Action<int> OnRoundEnded;
    public event Action<GamePhase> OnPhaseChanged;
    public event Action<int> OnDebtDue;
    public event Action<GameOverReason> OnGameOver;

    private float phaseTotalTime;
    private bool phaseHasTimer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        StartRound(1);
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

        if (PhaseTimeRemaining <= 0f)
        {
            if (CurrentPhase == GamePhase.Playing)
            {
                EndRound();
            }
            else if (CurrentPhase == GamePhase.Shop)
            {
                EndShopPhase();
            }
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

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 spawnPosition = GetRandomSpawnPoint();
            SpawnManager.Instance.SpawnChick(spawnPosition);
        }

        SetPhaseTimer(roundDuration);
    }

    private void EndRound()
    {
        OnRoundEnded?.Invoke(CurrentRound);
        EnterShopPhase();
    }

    private void EnterShopPhase()
    {
        SetPhase(GamePhase.Shop);
        SetPhaseTimer(roundConfig.ShopPhaseDuration);
    }

    public void EndShopPhase()
    {
        if (CurrentPhase != GamePhase.Shop)
        {
            return;
        }

        if (CurrentRound % roundConfig.DebtCheckpointInterval == 0)
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
        OnDebtDue?.Invoke(roundConfig.DebtAmount);
    }

    public void PayDebt()
    {
        if (CurrentPhase != GamePhase.DebtCheckpoint)
        {
            return;
        }

        if (EconomyManager.Instance.TrySpendGold(roundConfig.DebtAmount))
        {
            StartRound(CurrentRound + 1);
        }
        else
        {
            EnterGameOver(GameOverReason.NotEnoughGold);
        }
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
        OnPhaseChanged?.Invoke(CurrentPhase);
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
        return chickSpawnCenter + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
    }
}

public enum GameOverReason
{
    NotEnoughGold
}

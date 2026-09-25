using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugStatsPanel : MonoBehaviour
{
    private const float RefreshInterval = 0.5f;
    private const int MaxDetailLines = 20;

    [SerializeField] private TMP_Text label;
    [SerializeField] private Key toggleKey = Key.F1;
    [SerializeField] private Key endRoundKey = Key.None;
    [SerializeField] private Key spawnBatchKey = Key.F2;
    [SerializeField, Min(1)] private int spawnBatchCount = 500;
    [SerializeField] private Key spawnLargeBatchKey = Key.F3;
    [SerializeField, Min(1)] private int spawnLargeBatchCount = 2000;
    [SerializeField] private bool show = true;

    private readonly StringBuilder builder = new StringBuilder();

    private float refreshTimer;

    private void Start()
    {
        ApplyVisibility();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && toggleKey != Key.None && keyboard[toggleKey].wasPressedThisFrame)
        {
            show = !show;
            ApplyVisibility();
        }

        if (keyboard != null && endRoundKey != Key.None && keyboard[endRoundKey].wasPressedThisFrame)
        {
            GameManager.Instance?.EndRoundForDebug();
        }

        if (keyboard != null && Debug.isDebugBuild)
        {
            HandleSpawnKeys(keyboard);
        }

        if (!show || label == null)
        {
            return;
        }

        refreshTimer -= Time.unscaledDeltaTime;
        if (refreshTimer > 0f)
        {
            return;
        }

        refreshTimer = RefreshInterval;
        RefreshLabel();
    }

    private void HandleSpawnKeys(Keyboard keyboard)
    {
        if (spawnBatchKey != Key.None && keyboard[spawnBatchKey].wasPressedThisFrame)
        {
            SpawnManager.Instance?.SpawnDebugChicks(spawnBatchCount);
        }

        if (spawnLargeBatchKey != Key.None && keyboard[spawnLargeBatchKey].wasPressedThisFrame)
        {
            SpawnManager.Instance?.SpawnDebugChicks(spawnLargeBatchCount);
        }
    }

    private void RefreshLabel()
    {
        builder.Clear();

        IReadOnlyList<PlaceableEntity> entities = PlaceableEntity.ActiveEntities;

        int chickCount = SpawnManager.Instance != null ? SpawnManager.Instance.ChickCount : 0;
        int eggCount = SpawnManager.Instance != null ? SpawnManager.Instance.EggCount : 0;
        int chickenCount = SpawnManager.Instance != null ? SpawnManager.Instance.ChickenCount : 0;
        float fps = Time.unscaledDeltaTime > 0f ? 1f / Time.unscaledDeltaTime : 0f;

        builder.Append("Entities: ");
        builder.Append(entities.Count);
        builder.Append(" | Chick ");
        builder.Append(chickCount);
        builder.Append(" Egg ");
        builder.Append(eggCount);
        builder.Append(" Chicken ");
        builder.Append(chickenCount);
        builder.Append(" | ");
        builder.Append(fps.ToString("0"));
        builder.Append(" fps\n");

        int detailCount = Mathf.Min(entities.Count, MaxDetailLines);
        for (int i = 0; i < detailCount; i++)
        {
            AppendLine(entities[i]);
        }

        label.text = builder.ToString();
    }

    private void AppendLine(PlaceableEntity entity)
    {
        if (entity == null)
        {
            return;
        }

        builder.Append(entity.GetType().Name);
        builder.Append(": 성장 ");
        builder.Append(entity.CurrentGrowthInterval.ToString("0.0"));
        builder.Append("s / ");
        builder.Append(entity.CurrentGrowthChance.ToString("P0"));

        if (entity is Chicken chicken)
        {
            builder.Append(" | 산란 ");
            builder.Append(chicken.CurrentLayInterval.ToString("0.0"));
            builder.Append("s / ");
            builder.Append(chicken.CurrentLayChance.ToString("P0"));
        }

        builder.Append('\n');
    }

    private void ApplyVisibility()
    {
        if (label != null)
        {
            label.enabled = show;
        }
    }
}

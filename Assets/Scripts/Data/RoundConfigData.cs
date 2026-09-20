using UnityEngine;

[CreateAssetMenu(fileName = "RoundConfigData", menuName = "Round Config Data")]
public class RoundConfigData : ScriptableObject
{
    [Header("Round")]
    [SerializeField] private float roundDuration = 60f;
    [SerializeField] private int chickSpawnCountPerRound = 3;

    [Header("Debt")]
    [SerializeField] private int debtCheckpointInterval = 3;
    [SerializeField] private int debtAmount = 30;
    [SerializeField] private float debtMultiplier = 2.2f;

    [Header("Shop")]
    [SerializeField] private int rerollCost = 5;

    [Header("Chick Spawn")]
    [SerializeField] private float chickSpawnOuterRadius = 5f;
    [SerializeField] private float chickSpawnInnerRadius = 1.5f;
    [SerializeField] private float chickSpawnJitter = 0.25f;

    [Header("Day / Night Light")]
    [SerializeField] private Color dayLightColor = Color.white;
    [SerializeField] private float dayLightIntensity = 1f;
    [SerializeField] private Color sunsetLightColor = new Color(1f, 0.6f, 0.35f);
    [SerializeField] private float sunsetLightIntensity = 0.8f;
    [SerializeField] private Color nightLightColor = new Color(0.25f, 0.3f, 0.55f);
    [SerializeField] private float nightLightIntensity = 0.4f;
    [SerializeField] private float sunsetStartRatio = 0.5f;
    [SerializeField] private float nightStartRatio = 0.75f;
    [SerializeField] private float lightRestoreDuration = 2f;

    public float RoundDuration => roundDuration;
    public int ChickSpawnCountPerRound => chickSpawnCountPerRound;
    public int DebtCheckpointInterval => debtCheckpointInterval;
    public int DebtAmount => debtAmount;
    public float DebtMultiplier => debtMultiplier;
    public int RerollCost => rerollCost;
    public float ChickSpawnOuterRadius => chickSpawnOuterRadius;
    public float ChickSpawnInnerRadius => chickSpawnInnerRadius;
    public float ChickSpawnJitter => chickSpawnJitter;
    public Color DayLightColor => dayLightColor;
    public float DayLightIntensity => dayLightIntensity;
    public Color SunsetLightColor => sunsetLightColor;
    public float SunsetLightIntensity => sunsetLightIntensity;
    public Color NightLightColor => nightLightColor;
    public float NightLightIntensity => nightLightIntensity;
    public float SunsetStartRatio => sunsetStartRatio;
    public float NightStartRatio => nightStartRatio;
    public float LightRestoreDuration => lightRestoreDuration;
}

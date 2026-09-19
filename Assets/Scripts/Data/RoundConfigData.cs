using UnityEngine;

[CreateAssetMenu(fileName = "RoundConfigData", menuName = "Round Config Data")]
public class RoundConfigData : ScriptableObject
{
    [SerializeField] private float roundDuration = 60f;
    [SerializeField] private int chickSpawnCountPerRound = 3;
    [SerializeField] private float shopPhaseDuration = 20f;
    [SerializeField] private int debtCheckpointInterval = 3;
    [SerializeField] private int debtAmount = 30;
    [SerializeField] private float chickSpawnOuterRadius = 5f;
    [SerializeField] private float chickSpawnInnerRadius = 1.5f;

    public float RoundDuration => roundDuration;
    public int ChickSpawnCountPerRound => chickSpawnCountPerRound;
    public float ShopPhaseDuration => shopPhaseDuration;
    public int DebtCheckpointInterval => debtCheckpointInterval;
    public int DebtAmount => debtAmount;
    public float ChickSpawnOuterRadius => chickSpawnOuterRadius;
    public float ChickSpawnInnerRadius => chickSpawnInnerRadius;
}

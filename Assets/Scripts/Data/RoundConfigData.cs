using UnityEngine;

[CreateAssetMenu(fileName = "RoundConfigData", menuName = "Chick Check/Round Config")]
public class RoundConfigData : ScriptableObject
{
    [Header("Round")]
    [SerializeField, Min(1)] private float roundDuration = 60f;
    [SerializeField, Min(1)] private float tutorialRoundDuration = 60f;
    [SerializeField, Min(0)] private int chickSpawnCountPerRound = 3;

    [Header("Chick Spawn")]
    [SerializeField, Min(0)] private float chickSpawnOuterRadius = 3f;
    [SerializeField, Min(0)] private float chickSpawnInnerRadius = 2f;
    [SerializeField, Min(0)] private float chickSpawnJitter = 0.25f;

    public float RoundDuration => roundDuration;
    public float TutorialRoundDuration => tutorialRoundDuration;
    public int ChickSpawnCountPerRound => chickSpawnCountPerRound;

    public float ChickSpawnOuterRadius => chickSpawnOuterRadius;
    public float ChickSpawnInnerRadius => chickSpawnInnerRadius;
    public float ChickSpawnJitter => chickSpawnJitter;

    private void OnValidate()
    {
        chickSpawnInnerRadius = Mathf.Min(chickSpawnInnerRadius, chickSpawnOuterRadius);
    }
}

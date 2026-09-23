using UnityEngine;

[CreateAssetMenu(fileName = "RoundConfigData", menuName = "Chick Check/Round Config")]
public class RoundConfigData : ScriptableObject
{
    [Header("Round")]
    [SerializeField, Min(1)] private float roundDuration = 60f;
    [SerializeField, Min(0)] private int chickSpawnCountPerRound = 3;

    [Header("Chick Spawn")]
    [SerializeField, Min(0)] private float chickSpawnOuterRadius = 3f;
    [SerializeField, Min(0)] private float chickSpawnInnerRadius = 2f;
    [SerializeField, Min(0)] private float chickSpawnJitter = 0.25f;

    [Header("Tutorial")]
    [SerializeField, Range(0.05f, 1f)] private float tutorialRoundDurationRatio = 0.5f;

    public float RoundDuration => roundDuration;
    public int ChickSpawnCountPerRound => chickSpawnCountPerRound;
    public float ChickSpawnOuterRadius => chickSpawnOuterRadius;
    public float ChickSpawnInnerRadius => chickSpawnInnerRadius;
    public float ChickSpawnJitter => chickSpawnJitter;
    public float TutorialRoundDurationRatio => tutorialRoundDurationRatio;

    private void OnValidate()
    {
        chickSpawnInnerRadius = Mathf.Min(chickSpawnInnerRadius, chickSpawnOuterRadius);
    }
}

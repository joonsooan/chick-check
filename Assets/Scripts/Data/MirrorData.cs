using UnityEngine;

[CreateAssetMenu(fileName = "MirrorData", menuName = "Chick Colony/Mirror Data")]
public class MirrorData : ScriptableObject
{
    [Header("Duplication")]
    [SerializeField, Min(0)] private float dwellTime = 2f;
    [SerializeField, Min(0)] private float forcedExitDistance = 1.5f;
    [SerializeField, Min(0)] private float randomExitDuration = 0.5f;
    [SerializeField, Range(0f, 360f)] private float randomExitAngle = 60f;
    [SerializeField, Min(0)] private float exitSpeedMultiplier = 1.5f;
    [SerializeField, Min(0)] private float mutantExitSpeedMultiplier = 1.5f;

    [Header("Assist")]
    [SerializeField, Min(0)] private float assistDwellTime = 1.5f;

    public float DwellTime => dwellTime;
    public float ForcedExitDistance => forcedExitDistance;
    public float RandomExitDuration => randomExitDuration;
    public float RandomExitAngle => randomExitAngle;
    public float ExitSpeedMultiplier => exitSpeedMultiplier;
    public float MutantExitSpeedMultiplier => mutantExitSpeedMultiplier;
    public float AssistDwellTime => assistDwellTime;
}

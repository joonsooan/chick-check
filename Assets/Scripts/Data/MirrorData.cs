using UnityEngine;

[CreateAssetMenu(fileName = "MirrorData", menuName = "Mirror Data")]
public class MirrorData : ScriptableObject
{
    [Header("Duplication")]
    [SerializeField] private float dwellTime = 2f;
    [SerializeField] private float forcedExitDistance = 1.5f;
    [SerializeField] private float randomExitDuration = 0.5f;
    [SerializeField] private float randomExitAngle = 60f;
    [SerializeField] private float exitSpeedMultiplier = 1.5f;
    [SerializeField] private float mutantExitSpeedMultiplier = 1.5f;

    public float DwellTime => dwellTime;
    public float ForcedExitDistance => forcedExitDistance;
    public float RandomExitDuration => randomExitDuration;
    public float RandomExitAngle => randomExitAngle;
    public float ExitSpeedMultiplier => exitSpeedMultiplier;
    public float MutantExitSpeedMultiplier => mutantExitSpeedMultiplier;
}

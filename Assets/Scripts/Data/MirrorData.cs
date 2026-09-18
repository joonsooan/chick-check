using UnityEngine;

[CreateAssetMenu(fileName = "MirrorData", menuName = "Mirror Data")]
public class MirrorData : ScriptableObject
{
    [Header("Duplication")]
    [SerializeField] private float dwellTime = 2f;
    [SerializeField] private float exitDistance = 1.5f;

    public float DwellTime => dwellTime;
    public float ExitDistance => exitDistance;
}

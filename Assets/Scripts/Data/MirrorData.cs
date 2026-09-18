using UnityEngine;

[CreateAssetMenu(fileName = "MirrorData", menuName = "Mirror Data")]
public class MirrorData : ScriptableObject
{
    [Header("Duplication")]
    [SerializeField] private float duplicateInterval = 3f;
    [SerializeField] private float spawnScatterRadius = 0.3f;

    public float DuplicateInterval => duplicateInterval;
    public float SpawnScatterRadius => spawnScatterRadius;
}

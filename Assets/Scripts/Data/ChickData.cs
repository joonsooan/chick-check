using UnityEngine;

[CreateAssetMenu(fileName = "ChickData", menuName = "Chick Check/Chick Data")]
public class ChickData : EconomyEntityData
{
    [Header("Mutation")]
    [SerializeField] private bool isMutant;
    [SerializeField] private Chick mutatedPrefab;

    [Header("Alternate Growth")]
    [SerializeField] private GameObject alternateStagePrefab;
    [SerializeField, Range(0f, 1f)] private float alternateStageChance;

    public override EntityKind Kind => EntityKind.Chick;

    public bool IsMutant => isMutant;

    public Chick MutatedPrefab => mutatedPrefab;

    public GameObject DuckStagePrefab => alternateStagePrefab;
    public float DuckGrowthChance => alternateStageChance;
}

using UnityEngine;

[CreateAssetMenu(fileName = "ChickData", menuName = "Chick Colony/Chick Data")]
public class ChickData : EconomyEntityData
{
    [Header("Mutation")]
    [SerializeField] private bool isMutant;

    [Header("Alternate Growth")]
    [SerializeField] private GameObject alternateStagePrefab;
    [SerializeField, Range(0f, 1f)] private float alternateStageChance;

    public override EntityKind Kind => EntityKind.Chick;
    public bool IsMutant => isMutant;
    public GameObject AlternateStagePrefab => alternateStagePrefab;
    public float AlternateStageChance => alternateStageChance;
}

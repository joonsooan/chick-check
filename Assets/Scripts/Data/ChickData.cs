using UnityEngine;

[CreateAssetMenu(fileName = "ChickData", menuName = "Chick Colony/Chick Data")]
public class ChickData : EconomyEntityData
{
    [Header("Mutation")]
    [SerializeField] private bool isMutant;

    public override EntityKind Kind => EntityKind.Chick;
    public bool IsMutant => isMutant;
}

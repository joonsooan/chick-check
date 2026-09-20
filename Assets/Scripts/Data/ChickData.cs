using UnityEngine;

[CreateAssetMenu(fileName = "ChickData", menuName = "Chick Data")]
public class ChickData : EconomyEntityData
{
    [Header("Mutation")]
    [SerializeField] private bool isMutant;

    public bool IsMutant => isMutant;
}

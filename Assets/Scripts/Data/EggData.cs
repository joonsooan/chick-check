using UnityEngine;

[CreateAssetMenu(fileName = "EggData", menuName = "Egg Data")]
public class EggData : EconomyEntityData
{
    public override EntityKind Kind => EntityKind.Egg;
}

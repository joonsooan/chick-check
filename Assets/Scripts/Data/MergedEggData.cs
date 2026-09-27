using UnityEngine;

[CreateAssetMenu(fileName = "MergedEggData", menuName = "Chick Check/Merged Egg Data")]
public class MergedEggData : EggData
{
    public override EntityKind Kind => EntityKind.MergedEgg;
}

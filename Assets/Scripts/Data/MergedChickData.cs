using UnityEngine;

[CreateAssetMenu(fileName = "MergedChickData", menuName = "Chick Check/Merged Chick Data")]
public class MergedChickData : ChickData
{
    public override EntityKind Kind => EntityKind.MergedChick;
}

using UnityEngine;

[CreateAssetMenu(fileName = "MergedChickenData", menuName = "Chick Check/Merged Chicken Data")]
public class MergedChickenData : ChickenData
{
    public override EntityKind Kind => EntityKind.MergedChicken;
}

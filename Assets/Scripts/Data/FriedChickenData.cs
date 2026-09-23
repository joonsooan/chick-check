using UnityEngine;

[CreateAssetMenu(fileName = "FriedChickenData", menuName = "Chick Check/Fried Chicken Data")]
public class FriedChickenData : EconomyEntityData
{
    public override EntityKind Kind => EntityKind.FriedChicken;
}

using UnityEngine;

[CreateAssetMenu(fileName = "FriedChickenData", menuName = "Chick Colony/Fried Chicken Data")]
public class FriedChickenData : EconomyEntityData
{
    public override EntityKind Kind => EntityKind.FriedChicken;
}

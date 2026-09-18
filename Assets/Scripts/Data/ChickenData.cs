using UnityEngine;

[CreateAssetMenu(fileName = "ChickenData", menuName = "Chicken Data")]
public class ChickenData : EconomyEntityData
{
    [SerializeField] private Sprite sprite;

    public Sprite Sprite => sprite;
}

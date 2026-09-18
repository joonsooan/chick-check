using UnityEngine;

[CreateAssetMenu(fileName = "EggData", menuName = "Egg Data")]
public class EggData : EconomyEntityData
{
    [SerializeField] private Sprite sprite;

    public Sprite Sprite => sprite;
}

using UnityEngine;

[CreateAssetMenu(fileName = "ChickData", menuName = "Chick Data")]
public class ChickData : EconomyEntityData
{
    [SerializeField] private Sprite sprite;

    public Sprite Sprite => sprite;
}

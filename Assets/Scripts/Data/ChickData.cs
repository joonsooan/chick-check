using UnityEngine;

[CreateAssetMenu(fileName = "ChickData", menuName = "Chick Data")]
public class ChickData : EconomyEntityData
{
    [SerializeField] private Sprite sprite;

    [Header("Walk")]
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private Vector2 walkIntervalRange = new Vector2(1.5f, 4f);
    [SerializeField] private Vector2 walkDistanceRange = new Vector2(0.3f, 1f);

    public Sprite Sprite => sprite;
    public float MoveSpeed => moveSpeed;
    public Vector2 WalkIntervalRange => walkIntervalRange;
    public Vector2 WalkDistanceRange => walkDistanceRange;
}

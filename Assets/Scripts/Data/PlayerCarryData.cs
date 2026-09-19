using UnityEngine;

[CreateAssetMenu(fileName = "PlayerCarryData", menuName = "Player Carry Data")]
public class PlayerCarryData : ScriptableObject
{
    [Header("Pickup Range")]
    [SerializeField] private float carryRadius = 1.5f;

    [Header("Carry Position")]
    [SerializeField] private Vector2 carryOffset = new Vector2(0f, 0.8f);

    [Header("Pickup Animation")]
    [SerializeField] private float pickUpLockDuration = 0.4f;
    [SerializeField] private int pickUpGraspFrameIndex = 4;

    [Header("Drop Position")]
    [SerializeField] private float dropOffsetX = 0.5f;
    [SerializeField] private float dropOffsetYUp = 0.5f;
    [SerializeField] private float dropOffsetYDown = 0.5f;
    [SerializeField] private int dropMoveFrameIndex = 4;

    public float CarryRadius => carryRadius;
    public Vector2 CarryOffset => carryOffset;
    public float PickUpLockDuration => pickUpLockDuration;
    public int PickUpGraspFrameIndex => pickUpGraspFrameIndex;
    public float DropOffsetX => dropOffsetX;
    public float DropOffsetYUp => dropOffsetYUp;
    public float DropOffsetYDown => dropOffsetYDown;
    public int DropMoveFrameIndex => dropMoveFrameIndex;
}

using UnityEngine;

[CreateAssetMenu(fileName = "PlayerCarryData", menuName = "Chick Check/Player Carry Data")]
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

    [Header("Punch VFX")]
    [SerializeField] private Vector3 pickUpPunch = new Vector3(0.25f, 0.25f, 0f);
    [SerializeField, Min(0)] private float pickUpPunchDuration = 0.3f;
    [SerializeField] private Vector3 dropPunch = new Vector3(0.3f, -0.3f, 0f);
    [SerializeField, Min(0)] private float dropPunchDuration = 0.3f;

    public float CarryRadius => carryRadius;

    public Vector2 CarryOffset => carryOffset;

    public float PickUpLockDuration => pickUpLockDuration;
    public int PickUpGraspFrameIndex => pickUpGraspFrameIndex;

    public float DropOffsetX => dropOffsetX;
    public float DropOffsetYUp => dropOffsetYUp;
    public float DropOffsetYDown => dropOffsetYDown;
    public int DropMoveFrameIndex => dropMoveFrameIndex;

    public Vector3 PickUpPunch => pickUpPunch;
    public float PickUpPunchDuration => pickUpPunchDuration;
    public Vector3 DropPunch => dropPunch;
    public float DropPunchDuration => dropPunchDuration;
}

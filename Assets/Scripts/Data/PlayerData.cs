using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Chick Check/Player Data")]
public class PlayerData : ScriptableObject
{
    [SerializeField, Min(0.1f)] private float moveSpeed = 4f;
    [SerializeField, Min(1f)] private float sprintMultiplier = 1.5f;
    [SerializeField, Range(0.1f, 1f)] private float sprintFootstepIntervalRatio = 0.5f;

    public float MoveSpeed => moveSpeed;
    public float SprintMultiplier => sprintMultiplier;
    public float SprintFootstepIntervalRatio => sprintFootstepIntervalRatio;
}

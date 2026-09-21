using UnityEngine;

[CreateAssetMenu(fileName = "DebtConfigData", menuName = "Chick Colony/Debt Config")]
public class DebtConfigData : ScriptableObject
{
    [SerializeField, Min(1)] private int checkpointInterval = 3;
    [SerializeField, Min(0)] private int amount = 300;
    [SerializeField, Min(1f)] private float multiplier = 2.2f;

    public int CheckpointInterval => checkpointInterval;
    public int Amount => amount;
    public float Multiplier => multiplier;
}

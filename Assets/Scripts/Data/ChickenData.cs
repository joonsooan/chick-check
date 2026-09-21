using UnityEngine;

[CreateAssetMenu(fileName = "ChickenData", menuName = "Chicken Data")]
public class ChickenData : EconomyEntityData
{
    [Header("Egg Laying")]
    [SerializeField] private Egg eggPrefab;
    [SerializeField] private float layInterval = 5f;
    [SerializeField] private float layChance = 0.5f;
    [SerializeField] private Vector3 layOffset;
    [SerializeField] private int layCount = 1;

    public override EntityKind Kind => EntityKind.Chicken;
    public Egg EggPrefab => eggPrefab;
    public float LayInterval => layInterval;
    public float LayChance => layChance;
    public Vector3 LayOffset => layOffset;
    public int LayCount => layCount;
}

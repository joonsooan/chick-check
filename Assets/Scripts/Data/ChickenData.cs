using UnityEngine;

[CreateAssetMenu(fileName = "ChickenData", menuName = "Chick Check/Chicken Data")]
public class ChickenData : EconomyEntityData
{
    [Header("Egg Laying")]
    [SerializeField] private Egg eggPrefab;
    [SerializeField, Min(0.01f)] private float layInterval = 5f;
    [SerializeField, Range(0f, 1f)] private float layChance = 0.5f;
    [SerializeField] private Vector3 layOffset;
    [SerializeField, Min(1)] private int layCount = 1;

    [Header("Egg Placement")]
    [SerializeField, Min(1)] private int eggPlacementAttempts = 11;
    [SerializeField, Min(0)] private float eggPlacementRadius = 0.5f;

    [Header("Gold Egg")]
    [SerializeField] private Egg goldEggPrefab;
    [SerializeField, Range(0f, 1f)] private float goldEggChance;

    public override EntityKind Kind => EntityKind.Chicken;
    public Egg EggPrefab => eggPrefab;
    public float LayInterval => layInterval;
    public float LayChance => layChance;
    public Vector3 LayOffset => layOffset;
    public int LayCount => layCount;
    public int EggPlacementAttempts => eggPlacementAttempts;
    public float EggPlacementRadius => eggPlacementRadius;
    public Egg GoldEggPrefab => goldEggPrefab;
    public float GoldEggChance => goldEggChance;
}

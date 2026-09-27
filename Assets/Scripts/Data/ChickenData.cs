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
    [SerializeField, Min(0)] private float layPositionJitter = 0.15f;

    [Header("Lay VFX")]
    [SerializeField] private Vector3 layPunch = new Vector3(0.2f, -0.25f, 0f);
    [SerializeField, Min(0)] private float layPunchDuration = 0.3f;
    [SerializeField, Min(0)] private int layPunchVibrato = 6;
    [SerializeField, Range(0f, 1f)] private float layPunchElasticity = 0.6f;

    [Header("Egg Pop VFX")]
    [SerializeField] private Vector3 eggPopPunch = new Vector3(0.3f, 0.3f, 0f);
    [SerializeField, Min(0)] private float eggPopDuration = 0.35f;
    [SerializeField, Min(0)] private int eggPopVibrato = 8;
    [SerializeField, Range(0f, 1f)] private float eggPopElasticity = 0.8f;

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
    public float LayPositionJitter => layPositionJitter;

    public Vector3 LayPunch => layPunch;
    public float LayPunchDuration => layPunchDuration;
    public int LayPunchVibrato => layPunchVibrato;
    public float LayPunchElasticity => layPunchElasticity;

    public Vector3 EggPopPunch => eggPopPunch;
    public float EggPopDuration => eggPopDuration;
    public int EggPopVibrato => eggPopVibrato;
    public float EggPopElasticity => eggPopElasticity;

    public Egg GoldEggPrefab => goldEggPrefab;
    public float GoldEggChance => goldEggChance;
}

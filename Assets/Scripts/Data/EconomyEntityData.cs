using UnityEngine;

public abstract class EconomyEntityData : ScriptableObject
{
    [Header("Visual")]
    [SerializeField] private Sprite sprite;

    [Header("Economy")]
    [SerializeField] private int sellPrice;

    [Header("Growth")]
    [SerializeField] private float growthInterval = 5f;
    [SerializeField] private float growthChance = 0.1f;
    [SerializeField] private GameObject nextStagePrefab;

    [Header("Walk")]
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private Vector2 walkIntervalRange = new Vector2(1.5f, 4f);
    [SerializeField] private Vector2 walkDistanceRange = new Vector2(0.3f, 1f);

    public Sprite Sprite => sprite;
    public int SellPrice => sellPrice;
    public float GrowthInterval => growthInterval;
    public float GrowthChance => growthChance;
    public GameObject NextStagePrefab => nextStagePrefab;
    public bool CanGrow => nextStagePrefab != null;
    public float MoveSpeed => moveSpeed;
    public Vector2 WalkIntervalRange => walkIntervalRange;
    public Vector2 WalkDistanceRange => walkDistanceRange;
}

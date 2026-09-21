using UnityEngine;

public abstract class EconomyEntityData : ScriptableObject
{
    [Header("Visual")]
    [SerializeField] private Sprite sprite;

    [Header("Economy")]
    [SerializeField, Min(0)] private int sellPrice;

    [Header("Results")]
    [SerializeField, Min(0)] private int displayOrder;

    [Header("Growth")]
    [SerializeField, Min(0)] private float growthInterval = 5f;
    [SerializeField, Range(0f, 1f)] private float growthChance = 0.1f;
    [SerializeField] private GameObject nextStagePrefab;

    [Header("Walk")]
    [SerializeField, Min(0)] private float moveSpeed = 1f;
    [SerializeField] private Vector2 walkIntervalRange = new Vector2(1.5f, 4f);
    [SerializeField] private Vector2 walkDistanceRange = new Vector2(0.3f, 1f);

    [Header("Walk Target")]
    [SerializeField, Min(1)] private int walkTargetAttempts = 10;

    public abstract EntityKind Kind { get; }

    public Sprite Sprite => sprite;
    public int SellPrice => sellPrice;
    public int DisplayOrder => displayOrder;
    public float GrowthInterval => growthInterval;
    public float GrowthChance => growthChance;
    public GameObject NextStagePrefab => nextStagePrefab;
    public bool CanGrow => nextStagePrefab != null;
    public float MoveSpeed => moveSpeed;
    public Vector2 WalkIntervalRange => walkIntervalRange;
    public Vector2 WalkDistanceRange => walkDistanceRange;
    public int WalkTargetAttempts => walkTargetAttempts;

    private void OnValidate()
    {
        growthInterval = Mathf.Max(0f, growthInterval);
        moveSpeed = Mathf.Max(0f, moveSpeed);

        walkIntervalRange = new Vector2(
            Mathf.Max(0f, walkIntervalRange.x),
            Mathf.Max(Mathf.Max(0f, walkIntervalRange.x), walkIntervalRange.y));

        walkDistanceRange = new Vector2(
            Mathf.Max(0f, walkDistanceRange.x),
            Mathf.Max(Mathf.Max(0f, walkDistanceRange.x), walkDistanceRange.y));
    }
}

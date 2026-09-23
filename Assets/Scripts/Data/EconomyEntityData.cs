using UnityEngine;

public abstract class EconomyEntityData : ScriptableObject
{
    [Header("Visual")]
    [SerializeField] private Sprite sprite;
    [SerializeField] private SpriteAnimationData idleAnimation;
    [SerializeField] private SpriteAnimationData walkAnimation;

    [Header("Economy")]
    [SerializeField, Min(0)] private int sellPrice;

    [Header("Results")]
    [SerializeField, Min(0)] private int displayOrder;

    [Header("Growth")]
    [SerializeField, Min(0)] private float growthInterval = 5f;
    [SerializeField, Range(0f, 1f)] private float growthChance = 0.1f;
    [SerializeField] private Vector2 growthTimerJitterRange = new Vector2(0.1f, 0.2f);
    [SerializeField] private GameObject nextStagePrefab;

    [Header("Walk")]
    [SerializeField, Min(0)] private float moveSpeed = 1f;
    [SerializeField] private Vector2 walkIntervalRange = new Vector2(1.5f, 4f);
    [SerializeField] private Vector2 walkDistanceRange = new Vector2(0.3f, 1f);

    [Header("Walk Target")]
    [SerializeField, Min(1)] private int walkTargetAttempts = 10;

    [Header("Growth Pop VFX")]
    [SerializeField] private Vector3 growthPopPunch = new Vector3(0.3f, 0.3f, 0f);
    [SerializeField, Min(0)] private float growthPopDuration = 0.4f;
    [SerializeField, Min(0)] private int growthPopVibrato = 8;
    [SerializeField, Range(0f, 1f)] private float growthPopElasticity = 0.8f;

    public abstract EntityKind Kind { get; }

    public Sprite Sprite => sprite;
    public SpriteAnimationData IdleAnimation => idleAnimation;
    public SpriteAnimationData WalkAnimation => walkAnimation;
    public int SellPrice => sellPrice;
    public int DisplayOrder => displayOrder;
    public float GrowthInterval => growthInterval;
    public float GrowthChance => growthChance;
    public Vector2 GrowthTimerJitterRange => growthTimerJitterRange;
    public GameObject NextStagePrefab => nextStagePrefab;
    public bool CanGrow => nextStagePrefab != null;
    public float MoveSpeed => moveSpeed;
    public Vector2 WalkIntervalRange => walkIntervalRange;
    public Vector2 WalkDistanceRange => walkDistanceRange;
    public int WalkTargetAttempts => walkTargetAttempts;
    public Vector3 GrowthPopPunch => growthPopPunch;
    public float GrowthPopDuration => growthPopDuration;
    public int GrowthPopVibrato => growthPopVibrato;
    public float GrowthPopElasticity => growthPopElasticity;

    private void OnValidate()
    {
        growthInterval = Mathf.Max(0f, growthInterval);
        moveSpeed = Mathf.Max(0f, moveSpeed);

        growthTimerJitterRange = new Vector2(
            Mathf.Max(0f, growthTimerJitterRange.x),
            Mathf.Max(Mathf.Max(0f, growthTimerJitterRange.x), growthTimerJitterRange.y));

        walkIntervalRange = new Vector2(
            Mathf.Max(0f, walkIntervalRange.x),
            Mathf.Max(Mathf.Max(0f, walkIntervalRange.x), walkIntervalRange.y));

        walkDistanceRange = new Vector2(
            Mathf.Max(0f, walkDistanceRange.x),
            Mathf.Max(Mathf.Max(0f, walkDistanceRange.x), walkDistanceRange.y));
    }
}

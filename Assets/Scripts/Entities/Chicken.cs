using UnityEngine;

public class Chicken : PlaceableEntity
{
    [SerializeField] private ChickenData data;

    public override EconomyEntityData EconomyData => data;

    public override ShopStatType SellPriceStat => ShopStatType.SellPriceChicken;

    public float CurrentLayInterval { get; private set; }
    public float CurrentLayChance { get; private set; }

    private float layTimer;
    private float goldMineTimer;

    private int goldMineActiveLevel;
    private float cachedGoldMineInterval;
    private float cachedGoldMineChance;
    private int cachedGoldMineGoldAmount;

    private float cachedMultiLayChance;
    private int cachedMultiLayBonusCount;

    public override void RefreshStats()
    {
        base.RefreshStats();

        CurrentLayInterval = data.LayInterval;
        CurrentLayChance = ApplyModifier(ShopStatType.EggLayChance, data.LayChance);

        UpgradeManager manager = UpgradeManager.Instance;
        UpgradeState state = manager != null ? manager.State : null;

        goldMineActiveLevel = state != null ? state.GoldMineLevel : 0;
        cachedGoldMineInterval = goldMineActiveLevel > 0 ? state.GoldMineInterval : 1f;
        cachedGoldMineChance = state != null ? state.GoldMineChance : 0f;
        cachedGoldMineGoldAmount = state != null ? state.GoldMineGoldAmount : 0;

        cachedMultiLayChance = state != null ? state.MultiLayChance : 0f;
        cachedMultiLayBonusCount = state != null ? state.MultiLayBonusCount : 0;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        spriteRenderer.sprite = data.Sprite;
        ResetLifeState();
    }

    private void ResetLifeState()
    {
        ResetWalk(data);
        ResetAmbient(AmbientSoundKind.Chicken);
        layTimer = data.LayInterval;
        goldMineTimer = cachedGoldMineInterval;
    }

    public override void SetCarried(bool carried)
    {
        base.SetCarried(carried);

        if (carried)
        {
            if (animator != null)
            {
                animator.SetBool(IsWalkingHash, false);
            }
        }
        else if (gameObject.activeInHierarchy)
        {
            ResetLifeState();
        }
    }

    public override void Tick(float dt)
    {
        if (IsCarried)
        {
            return;
        }

        TickWalk(data, dt);
        TickAmbient(AmbientSoundKind.Chicken, dt);
        TickLayEgg(dt);
        TickGoldMine(dt);
        TickGrowth(dt);
    }

    private void TickLayEgg(float dt)
    {
        if (data.EggPrefab == null)
        {
            return;
        }

        layTimer -= dt;
        if (layTimer > 0f)
        {
            return;
        }

        layTimer = CurrentLayInterval;

        if (Random.value > CurrentLayChance)
        {
            return;
        }

        int layCount = data.LayCount;

        if (cachedMultiLayBonusCount > 0 && Random.value <= cachedMultiLayChance)
        {
            layCount += cachedMultiLayBonusCount;
        }

        for (int i = 0; i < layCount; i++)
        {
            SpawnEgg();
        }
    }

    private void SpawnEgg()
    {
        Vector3 spawnPosition = TryPickEggPosition(out Vector3 candidate) ? candidate : transform.position + data.LayOffset;
        PoolManager.Instance.Spawn(data.EggPrefab.gameObject, spawnPosition, Quaternion.identity, transform.parent);
        AudioManager.Instance?.PlayEggLaid();
    }

    private void TickGoldMine(float dt)
    {
        goldMineTimer -= dt;
        if (goldMineTimer > 0f)
        {
            return;
        }

        goldMineTimer = cachedGoldMineInterval;

        if (goldMineActiveLevel <= 0 || EconomyManager.Instance == null)
        {
            return;
        }

        if (Random.value <= cachedGoldMineChance)
        {
            EconomyManager.Instance.AddGold(cachedGoldMineGoldAmount);
        }
    }

    private bool TryPickEggPosition(out Vector3 position)
    {
        Vector3 basePosition = transform.position + data.LayOffset;

        if (GridManager.Instance == null)
        {
            position = basePosition;
            return true;
        }

        return GridManager.Instance.TryFindWalkable(basePosition, 11,
            attempt => attempt == 0 ? basePosition : basePosition + (Vector3)(Random.insideUnitCircle * 0.5f),
            out position);
    }
}

using UnityEngine;

public class Chicken : PlaceableEntity
{
    [SerializeField] private ChickenData data;

    public override EconomyEntityData EconomyData => data;

    public override ShopStatType SellPriceStat => ShopStatType.SellPriceChicken;

    protected override ShopStatType GrowthChanceStat => ShopStatType.ChickenGrowthChance;

    public float CurrentLayInterval { get; private set; }
    public float CurrentLayChance { get; private set; }
    public float CurrentGoldEggChance { get; private set; }

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
        CurrentGoldEggChance = ApplyModifier(ShopStatType.GoldEggChance, data.GoldEggChance);

        UpgradeManager manager = UpgradeManager.Instance;
        UpgradeState state = manager != null ? manager.State : null;

        goldMineActiveLevel = state != null ? state.GoldMineLevel : 0;
        cachedGoldMineInterval = goldMineActiveLevel > 0 ? state.GoldMineInterval : 0f;
        cachedGoldMineChance = state != null ? state.GoldMineChance : 0f;
        cachedGoldMineGoldAmount = state != null ? state.GoldMineGoldAmount : 0;

        cachedMultiLayChance = state != null ? state.MultiLayChance : 0f;
        cachedMultiLayBonusCount = state != null ? state.MultiLayBonusCount : 0;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

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
            flipbook.Play(data.IdleAnimation);
        }
        else if (gameObject.activeInHierarchy)
        {
            ResetLifeState();
        }
    }

    protected override void TickBehaviour(float dt)
    {
        TickWalk(data, dt);
        TickAmbient(AmbientSoundKind.Chicken, dt);
    }

    protected override void SlowTickBehaviour(float dt)
    {
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
        Egg eggPrefab = data.GoldEggPrefab != null && CurrentGoldEggChance > 0f && Random.value <= CurrentGoldEggChance
            ? data.GoldEggPrefab
            : data.EggPrefab;
        PoolManager.Instance.Spawn(eggPrefab.gameObject, spawnPosition, Quaternion.identity, transform.parent);
        AudioManager.Instance?.PlayEggLaid();
    }

    private void TickGoldMine(float dt)
    {
        if (goldMineActiveLevel <= 0)
        {
            return;
        }

        goldMineTimer -= dt;
        if (goldMineTimer > 0f)
        {
            return;
        }

        goldMineTimer = cachedGoldMineInterval;

        if (EconomyManager.Instance == null)
        {
            return;
        }

        if (Random.value <= cachedGoldMineChance)
        {
            EconomyManager.Instance.AddMinedGold(cachedGoldMineGoldAmount);
            AudioManager.Instance?.PlayGoldMine();
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

        return GridManager.Instance.TryFindWalkable(basePosition, data.EggPlacementAttempts,
            attempt => attempt == 0 ? basePosition : basePosition + (Vector3)(Random.insideUnitCircle * data.EggPlacementRadius),
            out position);
    }
}

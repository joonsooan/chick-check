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

    private int goldMineLevel;
    private float cachedGoldMineInterval;
    private float cachedGoldMineChance;
    private int cachedGoldMineGoldAmount;

    private float cachedMultiLayChance;
    private int cachedMultiLayBonusEggCount;

    public override void RefreshStats()
    {
        base.RefreshStats();

        CurrentLayInterval = data.LayInterval;
        CurrentLayChance = ApplyModifier(ShopStatType.EggLayChance, data.LayChance);
        CurrentGoldEggChance = ApplyModifier(ShopStatType.GoldEggChance, data.GoldEggChance);

        UpgradeManager manager = UpgradeManager.Instance;
        GoldMineState goldMine = manager != null ? manager.State.Get<GoldMineState>() : null;
        MultiLayState multiLay = manager != null ? manager.State.Get<MultiLayState>() : null;

        goldMineLevel = goldMine != null ? goldMine.Level : 0;
        cachedGoldMineInterval = goldMineLevel > 0 ? goldMine.Interval : 0f;
        cachedGoldMineChance = goldMine != null ? goldMine.Chance : 0f;
        cachedGoldMineGoldAmount = goldMine != null ? goldMine.GoldAmount : 0;

        cachedMultiLayChance = multiLay != null ? multiLay.Chance : 0f;
        cachedMultiLayBonusEggCount = multiLay != null ? multiLay.BonusEggCount : 0;
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

        if (cachedMultiLayBonusEggCount > 0 && Random.value <= cachedMultiLayChance)
        {
            layCount += cachedMultiLayBonusEggCount;
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
        if (goldMineLevel <= 0)
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

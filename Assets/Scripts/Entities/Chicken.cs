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

    protected override void RefreshDebugStats()
    {
        base.RefreshDebugStats();

        CurrentLayInterval = data.LayInterval;
        CurrentLayChance = ApplyModifier(ShopStatType.EggLayChance, data.LayChance);
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
        goldMineTimer = ResolveGoldMineInterval();
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

        CurrentLayInterval = data.LayInterval;
        CurrentLayChance = ApplyModifier(ShopStatType.EggLayChance, data.LayChance);

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

        if (UpgradeManager.Instance != null
            && UpgradeManager.Instance.MultiLayBonusCount > 0
            && Random.value <= UpgradeManager.Instance.MultiLayChance)
        {
            layCount += UpgradeManager.Instance.MultiLayBonusCount;
        }

        for (int i = 0; i < layCount; i++)
        {
            SpawnEgg();
        }
    }

    private void SpawnEgg()
    {
        Vector3 spawnPosition = TryPickEggPosition(out Vector3 candidate) ? candidate : transform.position + data.LayOffset;
        GameObject egg = PoolManager.Instance.Spawn(data.EggPrefab.gameObject, spawnPosition, Quaternion.identity, transform.parent);
        egg.GetComponent<PlaceableEntity>()?.NotifySpawnCount();
        AudioManager.Instance?.PlayEggLaid();
    }

    private float ResolveGoldMineInterval()
    {
        UpgradeManager manager = UpgradeManager.Instance;
        return manager != null && manager.GoldMineLevel > 0 ? manager.GoldMineInterval : 1f;
    }

    private void TickGoldMine(float dt)
    {
        goldMineTimer -= dt;
        if (goldMineTimer > 0f)
        {
            return;
        }

        goldMineTimer = ResolveGoldMineInterval();

        UpgradeManager manager = UpgradeManager.Instance;
        if (manager == null || manager.GoldMineLevel <= 0 || EconomyManager.Instance == null)
        {
            return;
        }

        if (Random.value <= manager.GoldMineChance)
        {
            EconomyManager.Instance.AddGold(manager.GoldMineGoldAmount);
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

    public override void NotifySpawnCount()
    {
        SpawnManager.Instance?.RegisterChickenSpawned();
    }

    public override void NotifyDespawnCount()
    {
        SpawnManager.Instance?.RegisterChickenDespawned();
    }
}

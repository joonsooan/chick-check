using System.Collections;
using UnityEngine;

public class Chicken : PlaceableEntity
{
    [SerializeField] private ChickenData data;

    public override EconomyEntityData EconomyData => data;

    public override ShopStatType SellPriceStat => ShopStatType.SellPriceChicken;

    public float CurrentLayInterval { get; private set; }
    public float CurrentLayChance { get; private set; }

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
        StartCoroutine(WalkRoutine(data));
        StartCoroutine(GrowthRoutine(data));
        StartCoroutine(LayEggRoutine());
        StartCoroutine(GoldMineRoutine());
        StartCoroutine(AmbientRoutine(AmbientSoundKind.Chicken));
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        StopAllCoroutines();
    }

    public override void SetCarried(bool carried)
    {
        base.SetCarried(carried);

        StopAllCoroutines();

        if (carried)
        {
            animator.SetBool(IsWalkingHash, false);
        }
        else
        {
            StartCoroutine(WalkRoutine(data));
            StartCoroutine(GrowthRoutine(data));
            StartCoroutine(LayEggRoutine());
            StartCoroutine(AmbientRoutine(AmbientSoundKind.Chicken));
        }
    }

    private IEnumerator LayEggRoutine()
    {
        if (data.EggPrefab == null)
        {
            yield break;
        }

        while (true)
        {
            CurrentLayInterval = data.LayInterval;
            CurrentLayChance = ApplyModifier(ShopStatType.EggLayChance, data.LayChance);

            yield return new WaitForSeconds(CurrentLayInterval);

            if (Random.value <= CurrentLayChance)
            {
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
        }
    }

    private void SpawnEgg()
    {
        Vector3 spawnPosition = TryPickEggPosition(out Vector3 candidate) ? candidate : transform.position + data.LayOffset;
        GameObject egg = PoolManager.Instance.Spawn(data.EggPrefab.gameObject, spawnPosition, Quaternion.identity, transform.parent);
        egg.GetComponent<PlaceableEntity>()?.NotifySpawnCount();
        AudioManager.Instance?.PlayEggLaid();
    }

    private IEnumerator GoldMineRoutine()
    {
        while (true)
        {
            UpgradeManager manager = UpgradeManager.Instance;
            float interval = manager != null && manager.GoldMineLevel > 0 ? manager.GoldMineInterval : 1f;

            yield return new WaitForSeconds(interval);

            if (manager == null || manager.GoldMineLevel <= 0 || EconomyManager.Instance == null)
            {
                continue;
            }

            if (Random.value <= manager.GoldMineChance)
            {
                EconomyManager.Instance.AddGold(manager.GoldMineGoldAmount);
            }
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

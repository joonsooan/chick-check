using System.Collections;
using UnityEngine;

public class Chicken : PlaceableEntity
{
    [SerializeField] private ChickenData data;

    public override EconomyEntityData EconomyData => data;

    protected override void Awake()
    {
        base.Awake();

        spriteRenderer.sprite = data.Sprite;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        StartCoroutine(WalkRoutine(data));
        StartCoroutine(GrowthRoutine(data));
        StartCoroutine(LayEggRoutine());
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
            float layInterval = data.LayInterval;
            float layChance = data.LayChance;
            if (UpgradeManager.Instance != null)
            {
                layInterval = UpgradeManager.Instance.ApplyModifiers(ShopStatType.EggLayInterval, layInterval);
                layChance = UpgradeManager.Instance.ApplyModifiers(ShopStatType.EggLayChance, layChance);
            }

            yield return new WaitForSeconds(layInterval);

            if (Random.value <= layChance)
            {
                for (int i = 0; i < data.LayCount; i++)
                {
                    Vector3 spawnPosition = TryPickEggPosition(out Vector3 candidate) ? candidate : transform.position + data.LayOffset;
                    GameObject egg = PoolManager.Instance.Spawn(data.EggPrefab.gameObject, spawnPosition, Quaternion.identity, transform.parent);
                    egg.GetComponent<PlaceableEntity>()?.NotifySpawnCount();
                }
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

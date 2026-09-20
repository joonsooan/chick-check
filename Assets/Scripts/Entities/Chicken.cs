using System.Collections;
using UnityEngine;

public class Chicken : PlaceableEntity
{
    [SerializeField] private ChickenData data;

    public override EconomyEntityData EconomyData => data;

    public float CurrentLayInterval { get; private set; }
    public float CurrentLayChance { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        spriteRenderer.sprite = data.Sprite;
    }

    protected override void RefreshDebugStats()
    {
        base.RefreshDebugStats();

        CurrentLayInterval = ApplyModifier(ShopStatType.EggLayInterval, data.LayInterval);
        CurrentLayChance = ApplyModifier(ShopStatType.EggLayChance, data.LayChance);
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
            CurrentLayInterval = ApplyModifier(ShopStatType.EggLayInterval, data.LayInterval);
            CurrentLayChance = ApplyModifier(ShopStatType.EggLayChance, data.LayChance);

            yield return new WaitForSeconds(CurrentLayInterval);

            if (Random.value <= CurrentLayChance)
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

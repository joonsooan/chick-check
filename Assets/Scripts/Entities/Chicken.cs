using UnityEngine;

public class Chicken : PlaceableEntity
{
    [SerializeField] private ChickenData data;

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
        }
    }

    protected override void NotifySpawnCount()
    {
        SpawnManager.Instance?.RegisterChickenSpawned();
    }

    protected override void NotifyDespawnCount()
    {
        SpawnManager.Instance?.RegisterChickenDespawned();
    }
}

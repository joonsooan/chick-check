using System.Collections;
using UnityEngine;

public class Chick : PlaceableEntity
{
    [SerializeField] private ChickData data;

    private Collider2D col;
    private bool colliderWasTrigger;

    public override EconomyEntityData EconomyData => data;
    public override bool CanBeCarried => base.CanBeCarried && !IsExiting;

    protected override void Awake()
    {
        base.Awake();

        spriteRenderer.sprite = data.Sprite;
        col = GetComponent<Collider2D>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        IsExiting = false;
        spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
        spriteRenderer.sortingLayerName = "Objects";
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

        if (!gameObject.activeInHierarchy)
        {
            return;
        }

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

    private void RestoreCollider()
    {
        if (col != null)
        {
            col.isTrigger = colliderWasTrigger;
        }
    }

    public override void NotifySpawnCount()
    {
        SpawnManager.Instance?.RegisterChickSpawned();
    }

    public override void NotifyDespawnCount()
    {
        SpawnManager.Instance?.RegisterChickDespawned();
    }

    public bool IsExiting { get; private set; }

    public void ForceExit(Vector2 dir, MirrorData mirrorData, bool masked)
    {
        StopAllCoroutines();
        StartCoroutine(ForceExitRoutine(dir.normalized, mirrorData, masked));
    }

    private IEnumerator ForceExitRoutine(Vector2 dir, MirrorData mirrorData, bool masked)
    {
        IsExiting = true;
        spriteRenderer.maskInteraction =
            masked ? SpriteMaskInteraction.VisibleOutsideMask : SpriteMaskInteraction.None;
        if (masked)
        {
            spriteRenderer.sortingLayerName = "Reflection";
        }

        if (col != null)
        {
            colliderWasTrigger = col.isTrigger;
            col.isTrigger = true;
        }

        animator.SetBool(IsWalkingHash, true);

        float moveSpeed = data.MoveSpeed * mirrorData.ExitSpeedMultiplier;

        yield return MoveForDistance(dir, moveSpeed, mirrorData.ForcedExitDistance);

        float randomAngle = Random.Range(-mirrorData.RandomExitAngle, mirrorData.RandomExitAngle);
        Vector2 randomDir = Quaternion.Euler(0f, 0f, randomAngle) * dir;
        yield return MoveForDuration(randomDir, moveSpeed, mirrorData.RandomExitDuration);

        spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
        if (masked)
        {
            spriteRenderer.sortingLayerName = "Objects";
        }
        RestoreCollider();
        IsExiting = false;
        StartCoroutine(WalkRoutine(data));
    }

    private IEnumerator MoveForDuration(Vector2 dir, float moveSpeed, float duration)
    {
        spriteRenderer.flipX = dir.x > 0f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator MoveForDistance(Vector2 dir, float moveSpeed, float distance)
    {
        spriteRenderer.flipX = dir.x > 0f;

        Vector3 target = transform.position + (Vector3)(dir * distance);
        while ((target - transform.position).sqrMagnitude > 0.0001f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }
}

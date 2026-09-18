using System.Collections;
using UnityEngine;

public class Chick : PlaceableEntity
{
    [SerializeField] private ChickData data;

    private Collider2D col;
    private bool colliderWasTrigger;

    public override EconomyEntityData EconomyData => data;

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

        if (carried)
        {
            if (IsExiting)
            {
                CancelForceExit();
            }

            animator.SetBool(IsWalkingHash, false);
        }
        else
        {
            StartCoroutine(WalkRoutine(data));
            StartCoroutine(GrowthRoutine(data));
        }
    }

    private void CancelForceExit()
    {
        IsExiting = false;
        spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
        spriteRenderer.sortingLayerName = "Objects";
        RestoreCollider();
    }

    private void RestoreCollider()
    {
        if (col != null)
        {
            col.isTrigger = colliderWasTrigger;
        }
    }

    protected override void NotifySpawnCount()
    {
        SpawnManager.Instance?.RegisterChickSpawned();
    }

    protected override void NotifyDespawnCount()
    {
        SpawnManager.Instance?.RegisterChickDespawned();
    }

    public bool IsExiting { get; private set; }

    public void ForceExit(Vector2 dir, float distance, bool masked)
    {
        StopAllCoroutines();
        StartCoroutine(ForceExitRoutine(dir.normalized, distance, masked));
    }

    private IEnumerator ForceExitRoutine(Vector2 dir, float distance, bool masked)
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

        Vector3 target = transform.position + (Vector3)(dir * distance);
        spriteRenderer.flipX = dir.x > 0f;
        animator.SetBool(IsWalkingHash, true);

        float moveSpeed = data.MoveSpeed;
        if (UpgradeManager.Instance != null)
        {
            moveSpeed = UpgradeManager.Instance.ApplyModifiers(ShopStatType.PlayerMoveSpeed, moveSpeed);
        }

        while ((target - transform.position).sqrMagnitude > 0.0001f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }

        spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
        if (masked)
        {
            spriteRenderer.sortingLayerName = "Objects";
        }
        RestoreCollider();
        IsExiting = false;
        StartCoroutine(WalkRoutine(data));
    }
}

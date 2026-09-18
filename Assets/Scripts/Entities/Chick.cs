using System.Collections;
using UnityEngine;

public class Chick : PlaceableEntity
{
    [SerializeField] private ChickData data;

    protected override void Awake()
    {
        base.Awake();

        spriteRenderer.sprite = data.Sprite;
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

        Vector3 target = transform.position + (Vector3)(dir * distance);
        spriteRenderer.flipX = dir.x > 0f;
        animator.SetBool(IsWalkingHash, true);

        while ((target - transform.position).sqrMagnitude > 0.0001f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, target, data.MoveSpeed * Time.deltaTime);
            yield return null;
        }

        spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
        if (masked)
        {
            spriteRenderer.sortingLayerName = "Objects";
        }
        IsExiting = false;
        StartCoroutine(WalkRoutine(data));
    }
}

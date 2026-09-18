using System.Collections;
using UnityEngine;

public class Chick : PlaceableEntity
{
    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");

    [SerializeField] private ChickData data;

    private Animator animator;

    protected override void Awake()
    {
        base.Awake();

        animator = GetComponent<Animator>();
        spriteRenderer.sprite = data.Sprite;
    }

    private void OnEnable()
    {
        IsExiting = false;
        spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
        spriteRenderer.sortingLayerName = "Objects";
        StartCoroutine(WalkRoutine());
        StartCoroutine(GrowthRoutine(data));
    }

    private void OnDisable()
    {
        StopAllCoroutines();
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
        StartCoroutine(WalkRoutine());
    }

    private IEnumerator WalkRoutine()
    {
        while (true)
        {
            animator.SetBool(IsWalkingHash, false);

            Vector2 interval = data.WalkIntervalRange;
            yield return new WaitForSeconds(Random.Range(interval.x, interval.y));

            if (!TryPickTarget(out Vector3 target))
            {
                continue;
            }

            spriteRenderer.flipX = target.x > transform.position.x;
            animator.SetBool(IsWalkingHash, true);

            while ((target - transform.position).sqrMagnitude > 0.0001f)
            {
                Vector3 next = Vector3.MoveTowards(
                    transform.position, target, data.MoveSpeed * Time.deltaTime);

                if (GridManager.Instance != null && !GridManager.Instance.IsWalkable(next))
                {
                    break;
                }

                transform.position = next;
                yield return null;
            }
        }
    }

    private bool TryPickTarget(out Vector3 target)
    {
        Vector2 distance = data.WalkDistanceRange;
        for (int attempt = 0; attempt < 10; attempt++)
        {
            Vector2 dir = Random.insideUnitCircle.normalized;
            Vector3 candidate = transform.position + (Vector3)(dir * Random.Range(distance.x, distance.y));

            if (GridManager.Instance == null || GridManager.Instance.IsWalkable(candidate))
            {
                target = candidate;
                return true;
            }
        }

        target = transform.position;
        return false;
    }
}

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
        StartCoroutine(WalkRoutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator WalkRoutine()
    {
        while (true)
        {
            animator.SetBool(IsWalkingHash, false);

            Vector2 interval = data.WalkIntervalRange;
            yield return new WaitForSeconds(Random.Range(interval.x, interval.y));

            Vector2 dir = Random.insideUnitCircle.normalized;
            Vector2 distance = data.WalkDistanceRange;
            Vector3 target = transform.position + (Vector3)(dir * Random.Range(distance.x, distance.y));

            if (dir.x != 0f)
            {
                spriteRenderer.flipX = dir.x > 0f;
            }

            animator.SetBool(IsWalkingHash, true);

            while ((target - transform.position).sqrMagnitude > 0.0001f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position, target, data.MoveSpeed * Time.deltaTime);
                yield return null;
            }
        }
    }
}

using System;
using UnityEngine;

public class EntityWalker
{
    private enum WalkState
    {
        Waiting,
        Moving
    }

    private readonly Transform transform;
    private readonly SpriteRenderer spriteRenderer;
    private readonly SpriteFlipbook flipbook;
    private readonly Func<Vector3, bool> isWalkable;
    private readonly Action onMoved;

    private WalkState state;
    private float timer;
    private Vector3 target;
    private bool guided;
    private bool ignoreObstacles;

    public EntityWalker(Transform transform, SpriteRenderer spriteRenderer, SpriteFlipbook flipbook, Func<Vector3, bool> isWalkable, Action onMoved = null)
    {
        this.transform = transform;
        this.spriteRenderer = spriteRenderer;
        this.flipbook = flipbook;
        this.isWalkable = isWalkable;
        this.onMoved = onMoved;
    }

    public bool IsMoving => state == WalkState.Moving;

    public void WalkTo(Vector3 destination, EconomyEntityData data, bool ignoreObstacles = false)
    {
        target = destination;
        this.ignoreObstacles = ignoreObstacles;
        spriteRenderer.flipX = target.x > transform.position.x;
        flipbook?.Play(data.WalkAnimation);
        state = WalkState.Moving;
        guided = true;
    }

    public void Reset(EconomyEntityData data)
    {
        flipbook?.Play(data.IdleAnimation);

        state = WalkState.Waiting;
        guided = false;
        ignoreObstacles = false;
        Vector2 interval = data.WalkIntervalRange;
        timer = UnityEngine.Random.Range(interval.x, interval.y);
    }

    public void Tick(EconomyEntityData data, float dt)
    {
        if (state == WalkState.Waiting)
        {
            timer -= dt;
            if (timer > 0f)
            {
                return;
            }

            if (!TryPickWalkTarget(data, out target))
            {
                Vector2 interval = data.WalkIntervalRange;
                timer = UnityEngine.Random.Range(interval.x, interval.y);
                return;
            }

            spriteRenderer.flipX = target.x > transform.position.x;
            flipbook?.Play(data.WalkAnimation);

            state = WalkState.Moving;
            return;
        }

        float moveSpeed = data.MoveSpeed;
        Vector3 current = transform.position;
        Vector3 next = Vector3.MoveTowards(current, target, moveSpeed * dt);

        if (IsBlocked(current, next))
        {
            Reset(data);
            return;
        }

        transform.position = next;
        onMoved?.Invoke();

        if ((target - next).sqrMagnitude <= 0.0001f)
        {
            Reset(data);
        }
    }

    private bool IsBlocked(Vector3 current, Vector3 next)
    {
        if (ignoreObstacles || isWalkable == null || isWalkable(next))
        {
            return false;
        }

        return !guided || isWalkable(current);
    }

    private bool TryPickWalkTarget(EconomyEntityData data, out Vector3 result)
    {
        Vector2 distance = data.WalkDistanceRange;
        Vector3 origin = transform.position;

        Vector3 RandomCandidate()
        {
            Vector2 dir = UnityEngine.Random.insideUnitCircle.normalized;
            return origin + (Vector3)(dir * UnityEngine.Random.Range(distance.x, distance.y));
        }

        if (isWalkable == null)
        {
            result = RandomCandidate();
            return true;
        }

        for (int attempt = 0; attempt < data.WalkTargetAttempts; attempt++)
        {
            Vector3 candidate = RandomCandidate();
            if (isWalkable(candidate))
            {
                result = candidate;
                return true;
            }
        }

        result = origin;
        return false;
    }
}

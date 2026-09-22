using System;
using UnityEngine;

public class MirrorExitMotion
{
    private enum Phase
    {
        None,
        Forced,
        Random
    }

    private readonly Func<float, float, float> randomRange;

    private Phase phase = Phase.None;
    private Vector3 forcedTarget;
    private Vector2 direction;
    private float moveSpeed;
    private float randomAngleRange;
    private float randomDuration;
    private float randomElapsed;

    public bool IsActive => phase != Phase.None;
    public Vector2 Direction => direction;

    public MirrorExitMotion(Func<float, float, float> randomRange = null)
    {
        this.randomRange = randomRange ?? DefaultRandomRange;
    }

    private static float DefaultRandomRange(float min, float max)
    {
        return UnityEngine.Random.Range(min, max);
    }

    public void Begin(Vector3 startPosition, Vector2 dir, float moveSpeed, float forcedDistance, float randomAngleRange, float randomDuration)
    {
        direction = dir.normalized;
        this.moveSpeed = moveSpeed;
        forcedTarget = startPosition + (Vector3)(direction * forcedDistance);
        this.randomAngleRange = randomAngleRange;
        this.randomDuration = randomDuration;
        randomElapsed = 0f;
        phase = Phase.Forced;
    }

    public void Cancel()
    {
        phase = Phase.None;
    }

    public bool Tick(float dt, ref Vector3 position)
    {
        if (phase == Phase.None)
        {
            return false;
        }

        if (phase == Phase.Forced)
        {
            Vector3 next = Vector3.MoveTowards(position, forcedTarget, moveSpeed * dt);
            position = next;

            if ((forcedTarget - next).sqrMagnitude > 0.0001f)
            {
                return false;
            }

            float randomAngle = randomRange(-randomAngleRange, randomAngleRange);
            direction = Quaternion.Euler(0f, 0f, randomAngle) * direction;
            randomElapsed = 0f;
            phase = Phase.Random;
            return true;
        }

        position += (Vector3)(direction * moveSpeed * dt);
        randomElapsed += dt;

        if (randomElapsed >= randomDuration)
        {
            phase = Phase.None;
        }

        return false;
    }
}

using UnityEngine;

public class AmbientVoice
{
    private float timer;

    public void Reset(AmbientSoundKind kind)
    {
        timer = Random.Range(ResolveRange(kind).x, ResolveRange(kind).y);
    }

    public void Tick(AmbientSoundKind kind, float dt, bool suppressed)
    {
        timer -= dt;
        if (timer > 0f)
        {
            return;
        }

        Vector2 range = ResolveRange(kind);
        timer = Random.Range(range.x, range.y);

        if (!suppressed)
        {
            AudioManager.Instance?.PlayAmbient(kind);
        }
    }

    private static Vector2 ResolveRange(AmbientSoundKind kind)
    {
        return AudioManager.Instance != null
            ? AudioManager.Instance.AmbientIntervalRange(kind)
            : new Vector2(4f, 9f);
    }
}

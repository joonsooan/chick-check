using UnityEngine;

public class AmbientVoice
{
    private float timer;
    private bool scheduled;

    public void Reset(AmbientSoundKind kind)
    {
        scheduled = false;
        Schedule(kind);
    }

    public void Tick(AmbientSoundKind kind, float dt, bool suppressed)
    {
        if (!scheduled)
        {
            Schedule(kind);
            return;
        }

        timer -= dt;
        if (timer > 0f)
        {
            return;
        }

        Schedule(kind);

        if (!suppressed)
        {
            AudioManager.Instance.PlayAmbient(kind);
        }
    }

    private void Schedule(AmbientSoundKind kind)
    {
        if (AudioManager.Instance == null)
        {
            return;
        }

        Vector2 range = AudioManager.Instance.AmbientIntervalRange(kind);
        timer = Random.Range(range.x, range.y);
        scheduled = true;
    }
}

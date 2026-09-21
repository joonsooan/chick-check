using UnityEngine;

public class SpriteFlipbook
{
    private readonly SpriteRenderer renderer;

    private int frame;
    private float elapsed;

    public SpriteFlipbook(SpriteRenderer renderer)
    {
        this.renderer = renderer;
    }

    public SpriteAnimationData Current { get; private set; }

    public int Frame => frame;

    public void Play(SpriteAnimationData clip)
    {
        if (clip == Current)
        {
            return;
        }

        Current = clip;
        frame = 0;
        elapsed = 0f;

        if (clip != null && clip.FrameCount > 0)
        {
            renderer.sprite = clip.Frames[0];
        }
    }

    public void Stop()
    {
        Current = null;
        frame = 0;
        elapsed = 0f;
    }

    public void Tick(float dt)
    {
        if (Current == null || Current.FrameCount == 0)
        {
            return;
        }

        int count = Current.FrameCount;
        float frameDuration = 1f / Current.FramesPerSecond;
        int startFrame = frame;

        elapsed += dt;
        while (elapsed >= frameDuration)
        {
            elapsed -= frameDuration;
            frame++;

            if (frame >= count)
            {
                if (Current.Loop)
                {
                    frame = 0;
                }
                else
                {
                    frame = count - 1;
                    elapsed = 0f;
                    break;
                }
            }
        }

        if (frame != startFrame)
        {
            renderer.sprite = Current.Frames[frame];
        }
    }
}

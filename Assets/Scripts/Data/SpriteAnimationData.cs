using UnityEngine;

[CreateAssetMenu(fileName = "SpriteAnimation", menuName = "Chick Check/Sprite Animation")]
public class SpriteAnimationData : ScriptableObject
{
    [SerializeField] private Sprite[] frames;
    [SerializeField, Min(0.01f)] private float framesPerSecond = 12f;
    [SerializeField] private bool loop = true;

    public Sprite[] Frames => frames;
    public float FramesPerSecond => framesPerSecond;
    public bool Loop => loop;
    public int FrameCount => frames != null ? frames.Length : 0;

#if UNITY_INCLUDE_TESTS
    public static SpriteAnimationData CreateForTest(Sprite[] frames, float fps, bool loop)
    {
        SpriteAnimationData instance = CreateInstance<SpriteAnimationData>();
        instance.frames = frames;
        instance.framesPerSecond = fps;
        instance.loop = loop;
        return instance;
    }
#endif
}

using UnityEngine;

public enum AmbientSoundKind
{
    Chick,
    Chicken
}

[CreateAssetMenu(fileName = "AudioLibrary", menuName = "Audio Library")]
public class AudioLibrary : ScriptableObject
{
    [Header("Master")]
    [Range(0f, 1f)][SerializeField] private float defaultSfxVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float defaultBgmVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float sfxVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float bgmVolume = 1f;
    [Range(0f, 0.5f)][SerializeField] private float sfxPitchVariance = 0.08f;

    [Header("BGM")]
    [SerializeField] private AudioClip titleBgm;
    [SerializeField] private AudioClip gameBgm;
    [SerializeField] private AudioClip resultBgm;
    [SerializeField, Min(0)] private float resultBgmFadeSeconds = 1.5f;
    [SerializeField, Min(0)] private float bgmCrossfadeSeconds = 0.75f;

    [Header("SFX - UI")]
    [SerializeField] private AudioClip uiClick;
    [SerializeField] private AudioClip digitMove;
    [SerializeField] private AudioClip pay;
    [SerializeField] private AudioClip settingsToggle;

    [Header("SFX - Shop")]
    [SerializeField] private AudioClip shopBuy;
    [SerializeField] private AudioClip shopReroll;

    [Header("SFX - Entity")]
    [SerializeField] private AudioClip eggLaid;
    [SerializeField] private AudioClip chickDuplicated;

    [Header("SFX - Round")]
    [SerializeField] private AudioClip roundEnd;
    [SerializeField] private AudioClip gameOver;
    [SerializeField] private AudioClip resultReveal;
    [SerializeField] private AudioClip resultRevealFinal;

    [Header("SFX - Player Footsteps")]
    [SerializeField] private AudioClip[] footsteps;
    [SerializeField, Min(0)] private float footstepInterval = 0.35f;

    [Header("SFX - Entity Ambient")]
    [SerializeField] private AudioClip[] chickAmbient;
    [SerializeField] private Vector2 chickAmbientIntervalRange = new Vector2(4f, 9f);
    [SerializeField] private AudioClip[] chickenAmbient;
    [SerializeField] private Vector2 chickenAmbientIntervalRange = new Vector2(5f, 11f);
    [SerializeField, Min(0)] private float ambientGlobalCooldown = 0.2f;

    public float DefaultSfxVolume => defaultSfxVolume;
    public float DefaultBgmVolume => defaultBgmVolume;
    public float SfxVolume => sfxVolume;
    public float BgmVolume => bgmVolume;
    public float SfxPitchVariance => sfxPitchVariance;

    public AudioClip TitleBgm => titleBgm;
    public AudioClip GameBgm => gameBgm;
    public AudioClip ResultBgm => resultBgm;
    public float ResultBgmFadeSeconds => resultBgmFadeSeconds;
    public float BgmCrossfadeSeconds => bgmCrossfadeSeconds;

    public AudioClip UiClick => uiClick;
    public AudioClip DigitMove => digitMove;
    public AudioClip Pay => pay;
    public AudioClip SettingsToggle => settingsToggle;

    public AudioClip ShopBuy => shopBuy;
    public AudioClip ShopReroll => shopReroll;

    public AudioClip EggLaid => eggLaid;
    public AudioClip ChickDuplicated => chickDuplicated;

    public AudioClip RoundEnd => roundEnd;
    public AudioClip GameOver => gameOver;
    public AudioClip ResultReveal => resultReveal;
    public AudioClip ResultRevealFinal => resultRevealFinal;

    public float FootstepInterval => footstepInterval;
    public float AmbientGlobalCooldown => ambientGlobalCooldown;

    public AudioClip RandomFootstep() => RandomFrom(footsteps);

    public AudioClip RandomAmbient(AmbientSoundKind kind) =>
        RandomFrom(kind == AmbientSoundKind.Chick ? chickAmbient : chickenAmbient);

    public Vector2 AmbientIntervalRange(AmbientSoundKind kind) =>
        kind == AmbientSoundKind.Chick ? chickAmbientIntervalRange : chickenAmbientIntervalRange;

    private static AudioClip RandomFrom(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0)
        {
            return null;
        }

        return clips[Random.Range(0, clips.Length)];
    }

    private void OnValidate()
    {
        chickAmbientIntervalRange = ClampRange(chickAmbientIntervalRange);
        chickenAmbientIntervalRange = ClampRange(chickenAmbientIntervalRange);
    }

    private static Vector2 ClampRange(Vector2 range)
    {
        float min = Mathf.Max(0f, range.x);
        return new Vector2(min, Mathf.Max(min, range.y));
    }
}

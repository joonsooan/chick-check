using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    public enum SceneBgm
    {
        None,
        Title,
        Game
    }

    private const string SfxVolumeKey = "audio_sfx_volume";
    private const string BgmVolumeKey = "audio_bgm_volume";

    [SerializeField] private AudioLibrary library;
    [SerializeField] private SceneBgm sceneBgm = SceneBgm.Game;

    private AudioSource sfxSource;
    private AudioSource fixedSfxSource;
    private AudioSource bgmSource;
    private float sfxVolume = 1f;
    private float bgmVolume = 1f;
    private float bgmFadeFactor = 1f;
    private Coroutine bgmFadeRoutine;
    private float lastAmbientTime = -999f;
    private readonly Dictionary<AudioClip, int> bgmResumeSamples = new Dictionary<AudioClip, int>();
    private readonly Dictionary<AudioClip, float> lastThrottledTime = new Dictionary<AudioClip, float>();
    private int budgetFrame = -1;
    private int budgetUsed;

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this)
        {
            return;
        }

        float sfxFallback = library != null ? library.DefaultSfxVolume : 1f;
        float bgmFallback = library != null ? library.DefaultBgmVolume : 1f;
        sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, sfxFallback);
        bgmVolume = PlayerPrefs.GetFloat(BgmVolumeKey, bgmFallback);

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        fixedSfxSource = gameObject.AddComponent<AudioSource>();
        fixedSfxSource.playOnAwake = false;

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
    }

    private void Start()
    {
        if (Instance != this)
        {
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundStarted += HandleRoundStarted;
            GameManager.Instance.OnRoundEnded += HandleRoundEnded;
            GameManager.Instance.OnGameOver += HandleGameOver;
        }

        PlaySceneBgm();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundStarted -= HandleRoundStarted;
            GameManager.Instance.OnRoundEnded -= HandleRoundEnded;
            GameManager.Instance.OnGameOver -= HandleGameOver;
        }
    }

    private void PlaySceneBgm()
    {
        if (library == null)
        {
            return;
        }

        switch (sceneBgm)
        {
            case SceneBgm.Title:
                PlayBgm(library.TitleBgm);
                break;
            case SceneBgm.Game:
                PlayBgm(library.GameBgm);
                break;
        }
    }

    public void PlaySfx(AudioClip clip, bool varyPitch = true)
    {
        if (clip == null || sfxSource == null || library == null)
        {
            return;
        }

        if (!varyPitch)
        {
            fixedSfxSource.pitch = 1f;
            fixedSfxSource.PlayOneShot(clip, sfxVolume * library.SfxVolume);
            return;
        }

        float variance = library.SfxPitchVariance;
        sfxSource.pitch = variance > 0f ? 1f + Random.Range(-variance, variance) : 1f;
        sfxSource.PlayOneShot(clip, sfxVolume * library.SfxVolume);
    }

    private void PlaySfxThrottled(AudioClip clip)
    {
        if (clip == null || library == null)
        {
            return;
        }

        if (lastThrottledTime.TryGetValue(clip, out float last) &&
            Time.unscaledTime - last < library.EntitySfxCooldown)
        {
            return;
        }

        if (budgetFrame != Time.frameCount)
        {
            budgetFrame = Time.frameCount;
            budgetUsed = 0;
        }

        if (budgetUsed >= library.SfxFrameBudget)
        {
            return;
        }

        budgetUsed++;
        lastThrottledTime[clip] = Time.unscaledTime;
        PlaySfx(clip);
    }

    public void PlayAmbient(AmbientSoundKind kind)
    {
        if (library == null || Time.unscaledTime - lastAmbientTime < library.AmbientGlobalCooldown)
        {
            return;
        }

        AudioClip clip = library.RandomAmbient(kind);
        if (clip == null)
        {
            return;
        }

        lastAmbientTime = Time.unscaledTime;
        PlaySfx(clip);
    }

    public Vector2 AmbientIntervalRange(AmbientSoundKind kind) =>
        library != null ? library.AmbientIntervalRange(kind) : new Vector2(4f, 9f);

    public float FootstepInterval => library != null ? library.FootstepInterval : 0.35f;

    public void PlayFootstep()
    {
        if (library != null)
        {
            PlaySfx(library.RandomFootstep());
        }
    }

    public void PlayRoundEndSfx()
    {
        if (library != null)
        {
            PlaySfx(library.RoundEnd, varyPitch: false);
        }
    }

    public void PlayGameOverSfx()
    {
        if (library != null)
        {
            PlaySfx(library.GameOver, varyPitch: false);
        }
    }

    public void PlayResultReveal()
    {
        if (library != null)
        {
            PlaySfx(library.ResultReveal);
        }
    }

    public void PlayResultRevealFinal()
    {
        if (library != null)
        {
            PlaySfx(library.ResultRevealFinal);
        }
    }

    public void PlayUiClick()
    {
        if (library != null)
        {
            PlaySfx(library.UiClick);
        }
    }

    public void PlayDigitMove()
    {
        if (library != null)
        {
            PlaySfx(library.DigitMove);
        }
    }

    public void PlayPay()
    {
        if (library != null)
        {
            PlaySfx(library.Pay);
        }
    }

    public void PlayEggLaid()
    {
        if (library != null)
        {
            PlaySfxThrottled(library.EggLaid);
        }
    }

    public void PlayChickDuplicated()
    {
        if (library != null)
        {
            PlaySfxThrottled(library.ChickDuplicated);
        }
    }

    public void PlaySettingsToggle()
    {
        if (library != null)
        {
            PlaySfx(library.SettingsToggle);
        }
    }

    public void PlayShopBuy()
    {
        if (library != null)
        {
            PlaySfx(library.ShopBuy);
        }
    }

    public void PlayShopReroll()
    {
        if (library != null)
        {
            PlaySfx(library.ShopReroll);
        }
    }

    public float SfxVolume => sfxVolume;
    public float BgmVolume => bgmVolume;

    public void SetSfxVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(SfxVolumeKey, sfxVolume);
    }

    public void SetBgmVolume(float value)
    {
        bgmVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(BgmVolumeKey, bgmVolume);
        ApplyBgmVolume();
    }

    private void ApplyBgmVolume()
    {
        if (bgmSource != null && library != null)
        {
            bgmSource.volume = bgmVolume * library.BgmVolume * bgmFadeFactor;
        }
    }

    private void PlayBgm(AudioClip clip)
    {
        if (bgmSource == null || clip == null || library == null)
        {
            return;
        }

        StopBgmFade();
        bgmFadeFactor = 1f;
        bgmSource.clip = clip;
        ApplyBgmVolume();
        bgmSource.Play();
    }

    public void FadeToBgm(AudioClip clip, float seconds, bool resume = false)
    {
        if (bgmSource == null || clip == null)
        {
            return;
        }

        StopBgmFade();
        bgmFadeRoutine = StartCoroutine(FadeToBgmRoutine(clip, seconds, resume));
    }

    private void SaveBgmPosition()
    {
        if (bgmSource != null && bgmSource.clip != null && bgmSource.isPlaying)
        {
            bgmResumeSamples[bgmSource.clip] = bgmSource.timeSamples;
        }
    }

    private void ApplyResumePosition(AudioClip clip, bool resume)
    {
        if (resume && bgmResumeSamples.TryGetValue(clip, out int samples))
        {
            bgmSource.timeSamples = Mathf.Clamp(samples, 0, clip.samples - 1);
        }
    }

    public void StopBgm(float fadeSeconds = 0f)
    {
        if (bgmSource == null)
        {
            return;
        }

        StopBgmFade();
        if (fadeSeconds <= 0f)
        {
            bgmSource.Stop();
            return;
        }

        bgmFadeRoutine = StartCoroutine(FadeOutRoutine(fadeSeconds));
    }

    private void StopBgmFade()
    {
        if (bgmFadeRoutine != null)
        {
            StopCoroutine(bgmFadeRoutine);
            bgmFadeRoutine = null;
        }
    }

    private IEnumerator FadeToBgmRoutine(AudioClip clip, float seconds, bool resume)
    {
        if (bgmSource.isPlaying && seconds > 0f)
        {
            float start = bgmFadeFactor;
            float t = 0f;
            while (t < seconds)
            {
                t += Time.unscaledDeltaTime;
                bgmFadeFactor = Mathf.Lerp(start, 0f, t / seconds);
                ApplyBgmVolume();
                yield return null;
            }
        }

        SaveBgmPosition();

        bgmFadeFactor = 0f;
        bgmSource.clip = clip;
        ApplyBgmVolume();
        bgmSource.Play();
        ApplyResumePosition(clip, resume);

        if (seconds > 0f)
        {
            float t = 0f;
            while (t < seconds)
            {
                t += Time.unscaledDeltaTime;
                bgmFadeFactor = Mathf.Lerp(0f, 1f, t / seconds);
                ApplyBgmVolume();
                yield return null;
            }
        }

        bgmFadeFactor = 1f;
        ApplyBgmVolume();
        bgmFadeRoutine = null;
    }

    private IEnumerator FadeOutRoutine(float seconds)
    {
        float start = bgmFadeFactor;
        float t = 0f;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            bgmFadeFactor = Mathf.Lerp(start, 0f, t / seconds);
            ApplyBgmVolume();
            yield return null;
        }

        bgmFadeFactor = 0f;
        bgmSource.Stop();
        bgmFadeRoutine = null;
    }

    private void HandleRoundStarted(int round)
    {
        if (library != null)
        {
            FadeToBgm(library.GameBgm, library.ResultBgmFadeSeconds, resume: true);
        }
    }

    private void HandleRoundEnded(int round)
    {
        PlayRoundEndSfx();
        if (library != null)
        {
            FadeToBgm(library.ResultBgm, library.BgmCrossfadeSeconds);
        }
    }

    private void HandleGameOver(GameOverReason reason)
    {
        StopBgm();
        PlayGameOverSfx();
    }
}

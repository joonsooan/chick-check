using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DayNightController : MonoBehaviour
{
    [SerializeField] private Light2D globalLight;
    [SerializeField] private RoundConfigData roundConfig;

    private bool isRestoring;
    private float restoreTimer;
    private Color restoreFromColor;
    private float restoreFromIntensity;

    private void Start()
    {
        ApplyLight(roundConfig.DayLightColor, roundConfig.DayLightIntensity);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundStarted += HandleRoundStarted;
            GameManager.Instance.OnPhaseTimerTick += HandlePhaseTimerTick;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundStarted -= HandleRoundStarted;
            GameManager.Instance.OnPhaseTimerTick -= HandlePhaseTimerTick;
        }
    }

    private void Update()
    {
        if (!isRestoring)
        {
            return;
        }

        restoreTimer += Time.deltaTime;
        float t = roundConfig.LightRestoreDuration > 0f
            ? Mathf.Clamp01(restoreTimer / roundConfig.LightRestoreDuration)
            : 1f;

        ApplyLight(
            Color.Lerp(restoreFromColor, roundConfig.DayLightColor, t),
            Mathf.Lerp(restoreFromIntensity, roundConfig.DayLightIntensity, t));

        if (t >= 1f)
        {
            isRestoring = false;
        }
    }

    private void HandleRoundStarted(int roundNumber)
    {
        restoreFromColor = globalLight != null ? globalLight.color : roundConfig.DayLightColor;
        restoreFromIntensity = globalLight != null ? globalLight.intensity : roundConfig.DayLightIntensity;
        restoreTimer = 0f;
        isRestoring = true;
    }

    private void HandlePhaseTimerTick(float remaining, float total)
    {
        if (total <= 0f)
        {
            return;
        }

        float progress = 1f - (remaining / total);
        if (progress < roundConfig.SunsetStartRatio)
        {
            return;
        }

        isRestoring = false;

        if (progress < roundConfig.NightStartRatio)
        {
            float t = Mathf.InverseLerp(roundConfig.SunsetStartRatio, roundConfig.NightStartRatio, progress);
            ApplyLight(
                Color.Lerp(roundConfig.DayLightColor, roundConfig.SunsetLightColor, t),
                Mathf.Lerp(roundConfig.DayLightIntensity, roundConfig.SunsetLightIntensity, t));
        }
        else
        {
            float t = Mathf.InverseLerp(roundConfig.NightStartRatio, 1f, progress);
            ApplyLight(
                Color.Lerp(roundConfig.SunsetLightColor, roundConfig.NightLightColor, t),
                Mathf.Lerp(roundConfig.SunsetLightIntensity, roundConfig.NightLightIntensity, t));
        }
    }

    private void ApplyLight(Color color, float intensity)
    {
        if (globalLight == null)
        {
            return;
        }

        globalLight.color = color;
        globalLight.intensity = intensity;
    }
}

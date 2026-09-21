using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DayNightController : MonoBehaviour
{
    [SerializeField] private Light2D globalLight;
    [SerializeField] private DayNightConfigData dayNightConfig;

    private bool isRestoring;
    private float restoreTimer;
    private Color restoreFromColor;
    private float restoreFromIntensity;

    private void Start()
    {
        ApplyLight(dayNightConfig.DayLightColor, dayNightConfig.DayLightIntensity);

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
        float t = dayNightConfig.LightRestoreDuration > 0f
            ? Mathf.Clamp01(restoreTimer / dayNightConfig.LightRestoreDuration)
            : 1f;

        ApplyLight(
            Color.Lerp(restoreFromColor, dayNightConfig.DayLightColor, t),
            Mathf.Lerp(restoreFromIntensity, dayNightConfig.DayLightIntensity, t));

        if (t >= 1f)
        {
            isRestoring = false;
        }
    }

    private void HandleRoundStarted(int roundNumber)
    {
        restoreFromColor = globalLight != null ? globalLight.color : dayNightConfig.DayLightColor;
        restoreFromIntensity = globalLight != null ? globalLight.intensity : dayNightConfig.DayLightIntensity;
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
        if (progress < dayNightConfig.SunsetStartRatio)
        {
            return;
        }

        isRestoring = false;

        if (progress < dayNightConfig.NightStartRatio)
        {
            float t = Mathf.InverseLerp(dayNightConfig.SunsetStartRatio, dayNightConfig.NightStartRatio, progress);
            ApplyLight(
                Color.Lerp(dayNightConfig.DayLightColor, dayNightConfig.SunsetLightColor, t),
                Mathf.Lerp(dayNightConfig.DayLightIntensity, dayNightConfig.SunsetLightIntensity, t));
        }
        else
        {
            float t = Mathf.InverseLerp(dayNightConfig.NightStartRatio, 1f, progress);
            ApplyLight(
                Color.Lerp(dayNightConfig.SunsetLightColor, dayNightConfig.NightLightColor, t),
                Mathf.Lerp(dayNightConfig.SunsetLightIntensity, dayNightConfig.NightLightIntensity, t));
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

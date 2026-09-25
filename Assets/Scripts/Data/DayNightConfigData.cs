using UnityEngine;

[CreateAssetMenu(fileName = "DayNightConfigData", menuName = "Chick Check/Day Night Config")]
public class DayNightConfigData : ScriptableObject
{
    [Header("Day")]
    [SerializeField, ColorUsage(false)] private Color dayLightColor = Color.white;
    [SerializeField, Min(0)] private float dayLightIntensity = 1f;

    [Header("Sunset")]
    [SerializeField, ColorUsage(false)] private Color sunsetLightColor = Color.white;
    [SerializeField, Min(0)] private float sunsetLightIntensity = 0.6f;

    [Header("Night")]
    [SerializeField, ColorUsage(false)] private Color nightLightColor = Color.white;
    [SerializeField, Min(0)] private float nightLightIntensity = 0.6f;

    [Header("Timing")]
    [SerializeField, Range(0f, 1f)] private float sunsetStartRatio = 0.4f;
    [SerializeField, Range(0f, 1f)] private float nightStartRatio = 0.8f;
    [SerializeField, Min(0)] private float lightRestoreDuration = 2f;

    public Color DayLightColor => dayLightColor;
    public float DayLightIntensity => dayLightIntensity;

    public Color SunsetLightColor => sunsetLightColor;
    public float SunsetLightIntensity => sunsetLightIntensity;

    public Color NightLightColor => nightLightColor;
    public float NightLightIntensity => nightLightIntensity;

    public float SunsetStartRatio => sunsetStartRatio;
    public float NightStartRatio => nightStartRatio;
    public float LightRestoreDuration => lightRestoreDuration;

    private void OnValidate()
    {
        nightStartRatio = Mathf.Max(nightStartRatio, sunsetStartRatio);
    }
}

using UnityEngine;

[CreateAssetMenu(fileName = "MutationData", menuName = "Chick Check/Mutation Data")]
public class MutationData : ScriptableObject
{
    [Header("Contagion")]
    [SerializeField, Min(0.01f)] private float contagionInterval = 3f;
    [SerializeField, Range(0f, 1f)] private float contagionChance = 0.2f;
    [SerializeField, Min(0)] private float contagionRadius = 2f;
    [SerializeField] private Vector2 contagionTimerJitterRange = new Vector2(0.1f, 0.2f);

    public float ContagionInterval => contagionInterval;
    public float ContagionChance => contagionChance;
    public float ContagionRadius => contagionRadius;
    public Vector2 ContagionTimerJitterRange => contagionTimerJitterRange;

    private void OnValidate()
    {
        contagionTimerJitterRange = new Vector2(
            Mathf.Max(0f, contagionTimerJitterRange.x),
            Mathf.Max(Mathf.Max(0f, contagionTimerJitterRange.x), contagionTimerJitterRange.y));
    }
}

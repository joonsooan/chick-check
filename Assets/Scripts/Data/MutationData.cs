using UnityEngine;

[CreateAssetMenu(fileName = "MutationData", menuName = "Chick Colony/Mutation Data")]
public class MutationData : ScriptableObject
{
    [Header("Mutant Chick")]
    [SerializeField] private Chick mutatedChickPrefab;

    [Header("Contagion")]
    [SerializeField, Min(0.01f)] private float contagionInterval = 3f;
    [SerializeField, Range(0f, 1f)] private float contagionChance = 0.2f;
    [SerializeField, Min(0)] private float contagionRadius = 2f;

    public Chick MutatedChickPrefab => mutatedChickPrefab;
    public float ContagionInterval => contagionInterval;
    public float ContagionChance => contagionChance;
    public float ContagionRadius => contagionRadius;
}

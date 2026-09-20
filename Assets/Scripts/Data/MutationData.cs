using UnityEngine;

[CreateAssetMenu(fileName = "MutationData", menuName = "Mutation Data")]
public class MutationData : ScriptableObject
{
    [Header("Mutant Chick")]
    [SerializeField] private Chick mutatedChickPrefab;

    [Header("Contagion")]
    [SerializeField] private float contagionInterval = 3f;
    [SerializeField] private float contagionChance = 0.2f;
    [SerializeField] private float contagionRadius = 2f;

    public Chick MutatedChickPrefab => mutatedChickPrefab;
    public float ContagionInterval => contagionInterval;
    public float ContagionChance => contagionChance;
    public float ContagionRadius => contagionRadius;
}

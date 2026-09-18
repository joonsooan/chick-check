using UnityEngine;

public abstract class EconomyEntityData : ScriptableObject
{
    [Header("Economy")]
    [SerializeField] private int sellPrice;

    [Header("Growth")]
    [SerializeField] private float growthInterval = 5f;
    [SerializeField] private float growthChance = 0.1f;
    [SerializeField] private GameObject nextStagePrefab;

    public int SellPrice => sellPrice;
    public float GrowthInterval => growthInterval;
    public float GrowthChance => growthChance;
    public GameObject NextStagePrefab => nextStagePrefab;
    public bool CanGrow => nextStagePrefab != null;
}

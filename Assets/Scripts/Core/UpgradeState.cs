using UnityEngine;

public class UpgradeState
{
    public StatModifierTable Modifiers { get; } = new StatModifierTable();

    public int MutantSpawnCount { get; internal set; }
    public bool ContagionEnabled { get; internal set; }
    public bool SprintEnabled { get; internal set; }

    public int GoldMineLevel { get; internal set; }
    public float GoldMineInterval { get; internal set; }
    public float GoldMineChance { get; internal set; }
    public int GoldMineBaseGold { get; internal set; }
    public int GoldMineGoldPerPurchase { get; internal set; }
    public int GoldMineGoldAmount =>
        GoldMineLevel <= 0 ? 0 : GoldMineBaseGold + GoldMineGoldPerPurchase * (GoldMineLevel - 1);

    public float MultiLayChance { get; internal set; }
    public int MultiLayBonusCount { get; internal set; }

    public GameObject PendingMirrorPrefab { get; private set; }

    internal void RequestMirrorUpgrade(GameObject prefab)
    {
        PendingMirrorPrefab = prefab;
    }

    internal GameObject ConsumePendingMirrorPrefab()
    {
        GameObject prefab = PendingMirrorPrefab;
        PendingMirrorPrefab = null;
        return prefab;
    }

    public void Reset()
    {
        Modifiers.Clear();
        MutantSpawnCount = 0;
        ContagionEnabled = false;
        SprintEnabled = false;
        GoldMineLevel = 0;
        GoldMineInterval = 0f;
        GoldMineChance = 0f;
        GoldMineBaseGold = 0;
        GoldMineGoldPerPurchase = 0;
        MultiLayChance = 0f;
        MultiLayBonusCount = 0;
        PendingMirrorPrefab = null;
    }
}

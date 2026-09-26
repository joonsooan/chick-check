using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ChickMergeEffect : ShopEffect
{
    [SerializeField] private ChickMergeData config;
    [SerializeField] private int recipeIndex;

    public override void Apply(UpgradeState state)
    {
        state.Get<ChickMergeState>().Unlock(recipeIndex);
    }

    public override bool IsAvailable(UpgradeState state)
    {
        if (config == null || recipeIndex < 0 || recipeIndex >= config.Recipes.Count)
        {
            return false;
        }

        ChickMergeData.MergeRecipe recipe = config.Recipes[recipeIndex];
        return state.Get<ChickMergeState>().GetPeak(recipeIndex) >= recipe.Count;
    }
}

public class ChickMergeState
{
    private readonly Dictionary<int, int> peak = new Dictionary<int, int>();
    private readonly HashSet<int> unlocked = new HashSet<int>();

    public void RecordCount(int recipeIndex, int count)
    {
        peak.TryGetValue(recipeIndex, out int current);
        if (count > current)
        {
            peak[recipeIndex] = count;
        }
    }

    public int GetPeak(int recipeIndex)
    {
        peak.TryGetValue(recipeIndex, out int value);
        return value;
    }

    internal void Unlock(int recipeIndex)
    {
        unlocked.Add(recipeIndex);
    }

    public bool IsUnlocked(int recipeIndex)
    {
        return unlocked.Contains(recipeIndex);
    }
}

using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ChickMergeController : MonoBehaviour
{
    private class ActiveMerge
    {
        public readonly List<PlaceableEntity> Sources = new List<PlaceableEntity>();
        public int RecipeIndex;
        public int Slot;
        public Vector3 Center;
        public bool ResultSpawned;
        public Sequence Sequence;
    }

    [SerializeField] private ChickMergeData data;
    [SerializeField] private Transform centerAnchor;
    [SerializeField] private Transform spawnParent;
    [SerializeField, Min(1)] private int centerSearchAttempts = 8;

    private readonly List<bool> unlockedCache = new List<bool>();
    private readonly List<PlaceableEntity> candidateBuffer = new List<PlaceableEntity>();
    private readonly List<Vector3> candidatePositions = new List<Vector3>();
    private readonly List<ActiveMerge> activeMerges = new List<ActiveMerge>();
    private readonly List<Vector3> slotCenters = new List<Vector3>();
    private readonly List<int> usedSlots = new List<int>();
    private readonly List<Vector3> groupCenterBuffer = new List<Vector3>();

    private bool MergeActive => activeMerges.Count > 0;

    private int MergingSourceCount
    {
        get
        {
            int sum = 0;
            for (int i = 0; i < activeMerges.Count; i++)
            {
                sum += activeMerges[i].Sources.Count;
            }

            return sum;
        }
    }

    private void Start()
    {
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnModifiersChanged += HandleModifiersChanged;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundEnding += HandleRoundEnding;
            GameManager.Instance.OnGameOver += HandleGameOver;
        }

        HandleModifiersChanged();
    }

    private void OnDestroy()
    {
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnModifiersChanged -= HandleModifiersChanged;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundEnding -= HandleRoundEnding;
            GameManager.Instance.OnGameOver -= HandleGameOver;
        }

        KillSequences();
    }

    private void HandleModifiersChanged()
    {
        unlockedCache.Clear();

        if (data == null || UpgradeManager.Instance == null)
        {
            return;
        }

        ChickMergeState state = UpgradeManager.Instance.State.Get<ChickMergeState>();
        IReadOnlyList<ChickMergeData.MergeRecipe> recipes = data.Recipes;
        for (int i = 0; i < recipes.Count; i++)
        {
            unlockedCache.Add(state.IsUnlocked(i));
        }
    }

    private void Update()
    {
        if (data == null)
        {
            return;
        }

        if (GameManager.Instance == null || GameManager.Instance.CurrentPhase != GamePhase.Playing)
        {
            return;
        }

        IReadOnlyList<ChickMergeData.MergeRecipe> recipes = data.Recipes;
        for (int i = 0; i < recipes.Count; i++)
        {
            if (i >= unlockedCache.Count || !unlockedCache[i])
            {
                continue;
            }

            ChickMergeData.MergeRecipe recipe = recipes[i];
            int freeGroups = recipe.MaxConcurrentMerges - activeMerges.Count;
            if (freeGroups <= 0 || !PassesCheapFilter(recipe))
            {
                continue;
            }

            CollectCandidates(recipe);
            int groups = MergeFormation.GroupCount(
                candidateBuffer.Count, recipe.Count, recipe.MinRemaining, freeGroups);
            if (groups <= 0)
            {
                continue;
            }

            StartMerges(i, recipe, groups);
            return;
        }
    }

    private bool PassesCheapFilter(ChickMergeData.MergeRecipe recipe)
    {
        if (SpawnManager.Instance == null)
        {
            return true;
        }

        int sum = SpawnManager.Instance.CountOf(recipe.SourceData);
        return sum - MergingSourceCount >= recipe.Count + recipe.MinRemaining;
    }

    private void CollectCandidates(ChickMergeData.MergeRecipe recipe)
    {
        candidateBuffer.Clear();

        IReadOnlyList<PlaceableEntity> entities = PlaceableEntity.ActiveEntities;
        for (int i = 0; i < entities.Count; i++)
        {
            PlaceableEntity entity = entities[i];
            if (entity.IsCarried || entity.IsMerging)
            {
                continue;
            }

            if (entity is Chick chick && chick.IsMirrorExiting)
            {
                continue;
            }

            if (!recipe.ContainsSource(entity.EconomyData))
            {
                continue;
            }

            candidateBuffer.Add(entity);
        }
    }

    private void StartMerges(int recipeIndex, ChickMergeData.MergeRecipe recipe, int groups)
    {
        candidatePositions.Clear();
        for (int i = 0; i < candidateBuffer.Count; i++)
        {
            candidatePositions.Add(candidateBuffer[i].transform.position);
        }

        if (!MergeActive)
        {
            slotCenters.Clear();
            slotCenters.AddRange(MergeFormation.GroupCenters(
                ResolveBaseCenter(recipe), recipe.MaxConcurrentMerges, recipe.GroupSpacing));
        }

        usedSlots.Clear();
        for (int i = 0; i < activeMerges.Count; i++)
        {
            usedSlots.Add(activeMerges[i].Slot);
        }

        List<int> freeSlots = MergeFormation.FreeSlots(slotCenters.Count, usedSlots, groups);

        groupCenterBuffer.Clear();
        for (int i = 0; i < freeSlots.Count; i++)
        {
            groupCenterBuffer.Add(slotCenters[freeSlots[i]]);
        }

        List<int[]> assignments = MergeFormation.AssignGroups(candidatePositions, groupCenterBuffer, recipe.Count);

        for (int g = 0; g < assignments.Count; g++)
        {
            int[] indices = assignments[g];
            if (indices.Length < recipe.Count)
            {
                continue;
            }

            Vector3 center = SnapToWalkable(groupCenterBuffer[g]);
            List<Vector3> ringPositions = MergeFormation.RingPositions(
                center, recipe.Count, recipe.RingBaseRadius, recipe.RingSpacing, recipe.MinArcSpacing);

            ActiveMerge merge = new ActiveMerge
            {
                RecipeIndex = recipeIndex,
                Slot = freeSlots[g],
                Center = center
            };

            for (int i = 0; i < indices.Length; i++)
            {
                PlaceableEntity source = candidateBuffer[indices[i]];
                merge.Sources.Add(source);
                source.BeginMerge(ringPositions[i], recipe.GatherDuration, recipe.GatherEase);
            }

            merge.Sequence = DOTween.Sequence();
            merge.Sequence.AppendInterval(recipe.GatherDuration + recipe.HoldAfterGather);
            merge.Sequence.AppendCallback(() => SpawnResult(merge));
            merge.Sequence.AppendInterval(recipe.HoldAfterSpawn);
            merge.Sequence.AppendCallback(() => FinishMerge(merge));

            activeMerges.Add(merge);
        }
    }

    private Vector3 ResolveBaseCenter(ChickMergeData.MergeRecipe recipe)
    {
        if (recipe.CenterMode != MergeCenterMode.SourceCentroid)
        {
            return FixedCenter;
        }

        Vector3 seed = MergeFormation.Centroid(candidatePositions);
        int[] nearest = MergeFormation.NearestIndices(candidatePositions, seed, recipe.Count);
        Vector3 centroid = MergeFormation.Centroid(candidatePositions, nearest);
        return recipe.DistanceFromAnchor > 0f
            ? MergeFormation.OffsetFrom(FixedCenter, centroid, recipe.DistanceFromAnchor)
            : centroid;
    }

    private Vector3 FixedCenter => centerAnchor != null ? centerAnchor.position : transform.position;

    private Vector3 SnapToWalkable(Vector3 fallback)
    {
        if (GridManager.Instance == null || GridManager.Instance.IsWalkable(fallback))
        {
            return fallback;
        }

        if (GridManager.Instance.TryFindWalkable(
            fallback,
            centerSearchAttempts,
            index => fallback + (Vector3)(Random.insideUnitCircle * (0.5f + index * 0.25f)),
            out Vector3 result))
        {
            return result;
        }

        return fallback;
    }

    private void SpawnResult(ActiveMerge merge)
    {
        if (merge.ResultSpawned || data == null || merge.RecipeIndex >= data.Recipes.Count)
        {
            return;
        }

        merge.ResultSpawned = true;

        ChickMergeData.MergeRecipe recipe = data.Recipes[merge.RecipeIndex];
        if (recipe.ResultPrefab == null)
        {
            return;
        }

        PlaceableEntity result = PoolManager.Instance.Spawn(
            recipe.ResultPrefab, merge.Center, Quaternion.identity, spawnParent);

        if (result != null)
        {
            result.PlayGrowthPop();
        }
    }

    private void FinishMerge(ActiveMerge merge)
    {
        DespawnSources(merge);
        merge.Sequence = null;
        activeMerges.Remove(merge);
    }

    private static void DespawnSources(ActiveMerge merge)
    {
        for (int i = 0; i < merge.Sources.Count; i++)
        {
            PlaceableEntity source = merge.Sources[i];
            if (source != null && source.IsRegistered)
            {
                source.Despawn();
            }
        }

        merge.Sources.Clear();
    }

    private void KillSequences()
    {
        for (int i = 0; i < activeMerges.Count; i++)
        {
            activeMerges[i].Sequence?.Kill(false);
            activeMerges[i].Sequence = null;
        }
    }

    private void HandleRoundEnding(int roundNumber)
    {
        if (!MergeActive)
        {
            return;
        }

        KillSequences();

        for (int i = 0; i < activeMerges.Count; i++)
        {
            SpawnResult(activeMerges[i]);
            DespawnSources(activeMerges[i]);
        }

        activeMerges.Clear();
    }

    private void HandleGameOver(GameOverReason reason)
    {
        KillSequences();

        for (int i = 0; i < activeMerges.Count; i++)
        {
            List<PlaceableEntity> sources = activeMerges[i].Sources;
            for (int j = 0; j < sources.Count; j++)
            {
                PlaceableEntity source = sources[j];
                if (source != null && source.IsRegistered)
                {
                    source.CancelMerge();
                }
            }

            sources.Clear();
        }

        activeMerges.Clear();
        unlockedCache.Clear();
    }
}

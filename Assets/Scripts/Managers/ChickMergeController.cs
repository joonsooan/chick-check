using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ChickMergeController : MonoBehaviour
{
    [SerializeField] private ChickMergeData data;
    [SerializeField] private Transform centerAnchor;
    [SerializeField] private Transform spawnParent;
    [SerializeField, Min(1)] private int centerSearchAttempts = 8;

    private readonly List<bool> unlockedCache = new List<bool>();
    private readonly List<PlaceableEntity> candidateBuffer = new List<PlaceableEntity>();
    private readonly List<Vector3> candidatePositions = new List<Vector3>();
    private readonly List<PlaceableEntity> activeSources = new List<PlaceableEntity>();

    private bool mergeActive;
    private bool resultSpawned;
    private int activeRecipeIndex;
    private Vector3 activeCenter;
    private PlaceableEntity spawnedResult;
    private Sequence activeSequence;

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

        activeSequence?.Kill();
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
        if (mergeActive || data == null)
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
            if (!PassesCheapFilter(recipe))
            {
                continue;
            }

            CollectCandidates(recipe);
            if (candidateBuffer.Count < recipe.Count + recipe.MinRemaining)
            {
                continue;
            }

            StartMerge(i, recipe);
            return;
        }
    }

    private bool PassesCheapFilter(ChickMergeData.MergeRecipe recipe)
    {
        if (SpawnManager.Instance == null)
        {
            return true;
        }

        int sum = 0;
        IReadOnlyList<EconomyEntityData> sourceData = recipe.SourceData;
        for (int i = 0; i < sourceData.Count; i++)
        {
            sum += SpawnManager.Instance.CountOf(sourceData[i]);
        }

        return sum >= recipe.Count + recipe.MinRemaining;
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

            if (!ContainsSource(recipe, entity.EconomyData))
            {
                continue;
            }

            candidateBuffer.Add(entity);
        }
    }

    private static bool ContainsSource(ChickMergeData.MergeRecipe recipe, EconomyEntityData data)
    {
        IReadOnlyList<EconomyEntityData> sourceData = recipe.SourceData;
        for (int i = 0; i < sourceData.Count; i++)
        {
            if (sourceData[i] == data)
            {
                return true;
            }
        }

        return false;
    }

    private void StartMerge(int recipeIndex, ChickMergeData.MergeRecipe recipe)
    {
        candidatePositions.Clear();
        for (int i = 0; i < candidateBuffer.Count; i++)
        {
            candidatePositions.Add(candidateBuffer[i].transform.position);
        }

        Vector3 seed = recipe.CenterMode == MergeCenterMode.SourceCentroid
            ? MergeFormation.Centroid(candidatePositions)
            : FixedCenter;

        int[] nearest = MergeFormation.NearestIndices(candidatePositions, seed, recipe.Count);

        Vector3 center = SnapToWalkable(recipe.CenterMode == MergeCenterMode.SourceCentroid
            ? ResolveCentroidCenter(MergeFormation.Centroid(candidatePositions, nearest), recipe.DistanceFromAnchor)
            : seed);
        List<Vector3> ringPositions = MergeFormation.RingPositions(
            center, recipe.Count, recipe.RingBaseRadius, recipe.RingSpacing, recipe.MinArcSpacing);

        activeSources.Clear();
        for (int i = 0; i < nearest.Length; i++)
        {
            PlaceableEntity source = candidateBuffer[nearest[i]];
            activeSources.Add(source);
            source.BeginMerge(ringPositions[i], recipe.GatherDuration, recipe.GatherEase);
        }

        mergeActive = true;
        resultSpawned = false;
        activeRecipeIndex = recipeIndex;
        activeCenter = center;
        spawnedResult = null;

        activeSequence = DOTween.Sequence();
        activeSequence.AppendInterval(recipe.GatherDuration + recipe.HoldAfterGather);
        activeSequence.AppendCallback(SpawnResult);
        activeSequence.AppendInterval(recipe.HoldAfterSpawn);
        activeSequence.AppendCallback(FinishMerge);
    }

    private Vector3 FixedCenter => centerAnchor != null ? centerAnchor.position : transform.position;

    private Vector3 ResolveCentroidCenter(Vector3 centroid, float distanceFromAnchor)
    {
        return distanceFromAnchor > 0f
            ? MergeFormation.OffsetFrom(FixedCenter, centroid, distanceFromAnchor)
            : centroid;
    }

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

    private void SpawnResult()
    {
        if (resultSpawned || data == null || activeRecipeIndex >= data.Recipes.Count)
        {
            return;
        }

        ChickMergeData.MergeRecipe recipe = data.Recipes[activeRecipeIndex];
        if (recipe.ResultPrefab == null)
        {
            resultSpawned = true;
            return;
        }

        PlaceableEntity result = PoolManager.Instance.Spawn(
            recipe.ResultPrefab, activeCenter, Quaternion.identity, spawnParent);

        if (result != null)
        {
            result.PlayGrowthPop();
        }

        spawnedResult = result;
        resultSpawned = true;
    }

    private void FinishMerge()
    {
        DespawnSources();

        mergeActive = false;
        resultSpawned = false;
        spawnedResult = null;
        activeSequence = null;
    }

    private void DespawnSources()
    {
        for (int i = 0; i < activeSources.Count; i++)
        {
            PlaceableEntity source = activeSources[i];
            if (source != null && source.IsRegistered)
            {
                source.Despawn();
            }
        }

        activeSources.Clear();
    }

    private void HandleRoundEnding(int roundNumber)
    {
        if (!mergeActive)
        {
            return;
        }

        activeSequence?.Kill(false);
        activeSequence = null;

        SpawnResult();
        DespawnSources();

        mergeActive = false;
        resultSpawned = false;
        spawnedResult = null;
    }

    private void HandleGameOver(GameOverReason reason)
    {
        activeSequence?.Kill(false);
        activeSequence = null;

        if (mergeActive)
        {
            for (int i = 0; i < activeSources.Count; i++)
            {
                PlaceableEntity source = activeSources[i];
                if (source != null && source.IsRegistered)
                {
                    source.CancelMerge();
                }
            }
        }

        activeSources.Clear();
        mergeActive = false;
        resultSpawned = false;
        spawnedResult = null;
        unlockedCache.Clear();
    }
}

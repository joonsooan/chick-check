using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public enum MergeCenterMode
{
    FixedTransform,
    SourceCentroid
}

[CreateAssetMenu(fileName = "ChickMergeData", menuName = "Chick Check/Chick Merge Data")]
public class ChickMergeData : ScriptableObject
{
    [Serializable]
    public class MergeRecipe
    {
        [SerializeField] private List<EconomyEntityData> sourceData = new List<EconomyEntityData>();
        [SerializeField, Min(1)] private int count = 100;
        [SerializeField, Min(0)] private int minRemaining = 1;
        [SerializeField] private PlaceableEntity resultPrefab;
        [SerializeField] private MergeCenterMode centerMode = MergeCenterMode.FixedTransform;
        [SerializeField, Min(0f)] private float distanceFromAnchor;
        [SerializeField, Min(0f)] private float ringBaseRadius = 0.6f;
        [SerializeField, Min(0f)] private float ringSpacing = 0.4f;
        [SerializeField, Min(0.01f)] private float minArcSpacing = 0.35f;
        [SerializeField, Min(0f)] private float gatherDuration = 1.2f;
        [SerializeField] private Ease gatherEase = Ease.OutCubic;
        [SerializeField, Min(0f)] private float holdAfterGather = 0.5f;
        [SerializeField, Min(0f)] private float holdAfterSpawn = 0.6f;

        public IReadOnlyList<EconomyEntityData> SourceData => sourceData;
        public int Count => count;
        public int MinRemaining => minRemaining;
        public PlaceableEntity ResultPrefab => resultPrefab;
        public MergeCenterMode CenterMode => centerMode;
        public float DistanceFromAnchor => distanceFromAnchor;
        public float RingBaseRadius => ringBaseRadius;
        public float RingSpacing => ringSpacing;
        public float MinArcSpacing => minArcSpacing;
        public float GatherDuration => gatherDuration;
        public Ease GatherEase => gatherEase;
        public float HoldAfterGather => holdAfterGather;
        public float HoldAfterSpawn => holdAfterSpawn;

        internal void Validate()
        {
            count = Mathf.Max(1, count);
            minRemaining = Mathf.Max(0, minRemaining);
            distanceFromAnchor = Mathf.Max(0f, distanceFromAnchor);
            ringBaseRadius = Mathf.Max(0f, ringBaseRadius);
            ringSpacing = Mathf.Max(0f, ringSpacing);
            minArcSpacing = Mathf.Max(0.01f, minArcSpacing);
            gatherDuration = Mathf.Max(0f, gatherDuration);
            holdAfterGather = Mathf.Max(0f, holdAfterGather);
            holdAfterSpawn = Mathf.Max(0f, holdAfterSpawn);
        }

#if UNITY_INCLUDE_TESTS
        internal static MergeRecipe CreateForTest(
            List<EconomyEntityData> sourceData,
            int count,
            int minRemaining,
            PlaceableEntity resultPrefab)
        {
            MergeRecipe recipe = new MergeRecipe
            {
                sourceData = sourceData,
                count = count,
                minRemaining = minRemaining,
                resultPrefab = resultPrefab
            };
            return recipe;
        }
#endif
    }

    [SerializeField] private List<MergeRecipe> recipes = new List<MergeRecipe>();

    public IReadOnlyList<MergeRecipe> Recipes => recipes;

    private void OnValidate()
    {
        foreach (MergeRecipe recipe in recipes)
        {
            recipe?.Validate();
        }
    }
}

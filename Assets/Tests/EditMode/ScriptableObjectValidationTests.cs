using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

[TestFixture]
public class ScriptableObjectValidationTests
{
    private static bool IsTestDataPath(string path)
    {
        return path.Contains("/TestData/") || path.Contains("/Test Data/");
    }

    [Test]
    public void EconomyEntityData_AllAssetsHaveValidFields()
    {
        string[] guids = AssetDatabase.FindAssets("t:EconomyEntityData");
        Assert.IsNotEmpty(guids, "No EconomyEntityData assets found.");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            EconomyEntityData data = AssetDatabase.LoadAssetAtPath<EconomyEntityData>(path);
            Assert.IsNotNull(data, $"Failed to load {path}");

            Assert.GreaterOrEqual(data.SellPrice, 0, $"{path} has negative SellPrice");
            if (data.HasNextStage)
            {
                Assert.Greater(data.GrowthInterval, 0f, $"{path} can grow but has non-positive GrowthInterval");
                Assert.IsTrue(
                    data.NextStagePrefab.GetComponent<PlaceableEntity>() != null,
                    $"{path}.NextStagePrefab is missing a PlaceableEntity component");
            }
        }
    }

    [Test]
    public void EconomyEntityData_DisplayOrderIsUniquePerKind()
    {
        Dictionary<EntityKind, Dictionary<int, string>> seenByKind = new Dictionary<EntityKind, Dictionary<int, string>>();

        foreach (string guid in AssetDatabase.FindAssets("t:EconomyEntityData"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (IsTestDataPath(path))
            {
                continue;
            }

            EconomyEntityData data = AssetDatabase.LoadAssetAtPath<EconomyEntityData>(path);
            if (!seenByKind.TryGetValue(data.Kind, out Dictionary<int, string> seen))
            {
                seen = new Dictionary<int, string>();
                seenByKind[data.Kind] = seen;
            }

            if (seen.TryGetValue(data.DisplayOrder, out string existingPath))
            {
                Assert.Fail($"Duplicate DisplayOrder {data.DisplayOrder} for {data.Kind} at {path} and {existingPath}");
            }

            seen[data.DisplayOrder] = path;
        }
    }

    [Test]
    public void ChickenData_GoldEggConfigurationIsValid()
    {
        string[] guids = AssetDatabase.FindAssets("t:ChickenData");
        Assert.IsNotEmpty(guids, "No ChickenData assets found.");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ChickenData data = AssetDatabase.LoadAssetAtPath<ChickenData>(path);

            Assert.GreaterOrEqual(data.GoldEggChance, 0f, $"{path} has GoldEggChance below 0");
            Assert.LessOrEqual(data.GoldEggChance, 1f, $"{path} has GoldEggChance above 1");

            if (data.HasNextStage)
            {
                PlaceableEntity nextStage = data.NextStagePrefab.GetComponent<PlaceableEntity>();
                Assert.IsFalse(nextStage is Chicken, $"{path}.NextStagePrefab must not grow back into a Chicken");
            }
        }
    }

    [Test]
    public void EggData_IdleAnimationsHaveFrames()
    {
        foreach (string guid in AssetDatabase.FindAssets("t:EggData"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (IsTestDataPath(path))
            {
                continue;
            }

            EggData data = AssetDatabase.LoadAssetAtPath<EggData>(path);
            if (data.IdleAnimation == null)
            {
                continue;
            }

            Assert.Greater(data.IdleAnimation.FrameCount, 0, $"{path} IdleAnimation has no frames");
        }
    }

    [Test]
    public void ChickData_DuckStagePrefabHasPlaceableEntity()
    {
        foreach (string guid in AssetDatabase.FindAssets("t:ChickData"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (IsTestDataPath(path))
            {
                continue;
            }

            ChickData data = AssetDatabase.LoadAssetAtPath<ChickData>(path);
            if (data.DuckStagePrefab == null)
            {
                continue;
            }

            Assert.IsTrue(
                data.DuckStagePrefab.GetComponent<PlaceableEntity>() != null,
                $"{path}.DuckStagePrefab is missing a PlaceableEntity component");
        }
    }

    [Test]
    public void ChickData_MutationChainIsValid()
    {
        foreach (string guid in AssetDatabase.FindAssets("t:ChickData"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (IsTestDataPath(path))
            {
                continue;
            }

            ChickData data = AssetDatabase.LoadAssetAtPath<ChickData>(path);

            if (data.IsMutant)
            {
                Assert.IsTrue(data.MutatedPrefab == null, $"{path} is already a mutant but has a MutatedPrefab assigned");
                continue;
            }

            if (data.MutatedPrefab == null)
            {
                continue;
            }

            ChickData mutatedData = data.MutatedPrefab.EconomyData as ChickData;
            Assert.IsNotNull(mutatedData, $"{path}.MutatedPrefab has no ChickData assigned");
            Assert.IsTrue(mutatedData.IsMutant, $"{path}.MutatedPrefab does not have IsMutant set");
            Assert.AreEqual(data.Kind, mutatedData.Kind, $"{path}.MutatedPrefab must be the same Kind as the source");
        }
    }

    [Test]
    public void MergedChickenData_DoesNotGrow()
    {
        foreach (string guid in AssetDatabase.FindAssets("t:MergedChickenData"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (IsTestDataPath(path))
            {
                continue;
            }

            MergedChickenData data = AssetDatabase.LoadAssetAtPath<MergedChickenData>(path);

            Assert.IsFalse(data.HasNextStage, $"{path} must not have a NextStagePrefab");
            Assert.IsTrue(data.GoldEggPrefab == null, $"{path} must not have a GoldEggPrefab");

            if (data.EggPrefab != null)
            {
                Assert.IsTrue(data.EggPrefab is MergedEgg, $"{path}.EggPrefab must be a MergedEgg");
            }
        }
    }

    [Test]
    public void MergedEggData_HatchesIntoMergedChick()
    {
        foreach (string guid in AssetDatabase.FindAssets("t:MergedEggData"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (IsTestDataPath(path))
            {
                continue;
            }

            MergedEggData data = AssetDatabase.LoadAssetAtPath<MergedEggData>(path);
            if (data.NextStagePrefab == null)
            {
                continue;
            }

            Assert.IsTrue(
                data.NextStagePrefab.GetComponent<MergedChick>() != null,
                $"{path}.NextStagePrefab must be a MergedChick");
        }
    }

    [Test]
    public void DayNightConfigData_SunsetStartsBeforeNight()
    {
        string[] guids = AssetDatabase.FindAssets("t:DayNightConfigData");
        Assert.IsNotEmpty(guids, "No DayNightConfigData assets found.");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            DayNightConfigData data = AssetDatabase.LoadAssetAtPath<DayNightConfigData>(path);
            Assert.LessOrEqual(data.SunsetStartRatio, data.NightStartRatio, $"{path} sunset must not start after night");
        }
    }

    [Test]
    public void DebtConfigData_DebtGrowthOnPayoffIsAtLeastOne()
    {
        string[] guids = AssetDatabase.FindAssets("t:DebtConfigData");
        Assert.IsNotEmpty(guids, "No DebtConfigData assets found.");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            DebtConfigData data = AssetDatabase.LoadAssetAtPath<DebtConfigData>(path);
            Assert.GreaterOrEqual(data.DebtGrowthOnPayoff, 1f, $"{path} has DebtGrowthOnPayoff below 1");
        }
    }

    [Test]
    public void ShopItemData_AllAssetsHaveValidFields()
    {
        string[] guids = AssetDatabase.FindAssets("t:ShopItemData");
        Assert.IsNotEmpty(guids, "No ShopItemData assets found.");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ShopItemData item = AssetDatabase.LoadAssetAtPath<ShopItemData>(path);
            Assert.IsNotNull(item, $"Failed to load {path}");

            Assert.Greater(item.Cost, 0, $"{path} has non-positive Cost");
            Assert.Greater(item.CostGrowthPerPurchase, 0f, $"{path} has non-positive CostGrowthPerPurchase");
            Assert.IsFalse(string.IsNullOrEmpty(item.ItemName), $"{path} has empty ItemName");
            Assert.IsTrue(item.Effect != null, $"{path} has no Effect assigned");
        }
    }

    [Test]
    public void ShopItemData_ItemNamesAreUnique()
    {
        HashSet<string> names = new HashSet<string>();
        foreach (string guid in AssetDatabase.FindAssets("t:ShopItemData"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ShopItemData item = AssetDatabase.LoadAssetAtPath<ShopItemData>(path);
            Assert.IsTrue(names.Add(item.ItemName), $"Duplicate ItemName '{item.ItemName}' at {path}");
        }
    }

    [Test]
    public void ShopConfigData_CatalogIsValid()
    {
        string[] guids = AssetDatabase.FindAssets("t:ShopConfigData");
        Assert.IsNotEmpty(guids, "No ShopConfigData assets found.");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ShopConfigData config = AssetDatabase.LoadAssetAtPath<ShopConfigData>(path);
            Assert.IsNotNull(config, $"Failed to load {path}");

            IReadOnlyList<ShopItemData> catalog = config.Catalog;
            Assert.IsNotNull(catalog, $"{path} has a null catalog");

            HashSet<ShopItemData> seen = new HashSet<ShopItemData>();
            foreach (ShopItemData item in catalog)
            {
                Assert.IsTrue(item != null, $"{path} catalog contains a null entry");
                Assert.IsTrue(seen.Add(item), $"{path} catalog contains duplicate entry '{item.ItemName}'");
            }

            Assert.GreaterOrEqual(catalog.Count, config.ItemsPerRound,
                $"{path} catalog has fewer entries than ItemsPerRound");

            foreach (ShopItemData item in catalog)
            {
                string itemPath = AssetDatabase.GetAssetPath(item);
                string fileName = System.IO.Path.GetFileNameWithoutExtension(itemPath);

                Assert.IsTrue(fileName.StartsWith("Upgrade_"), $"{itemPath} filename must start with 'Upgrade_'");
                if (item.OneTimeOnly)
                {
                    Assert.IsTrue(fileName.EndsWith("_OneTime"), $"{itemPath} is OneTimeOnly but filename lacks '_OneTime' suffix");
                }
            }
        }
    }

    [Test]
    public void EntityData_ChickAndChickenHaveAnimations()
    {
        List<string> guids = new List<string>();
        guids.AddRange(AssetDatabase.FindAssets("t:ChickData"));
        guids.AddRange(AssetDatabase.FindAssets("t:ChickenData"));

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (IsTestDataPath(path))
            {
                continue;
            }

            EconomyEntityData data = AssetDatabase.LoadAssetAtPath<EconomyEntityData>(path);
            Assert.IsTrue(data.IdleAnimation != null, $"{path} has no IdleAnimation assigned");
            Assert.IsTrue(data.WalkAnimation != null, $"{path} has no WalkAnimation assigned");

            Assert.Greater(data.IdleAnimation.FrameCount, 0, $"{path} IdleAnimation has no frames");
            Assert.Greater(data.WalkAnimation.FrameCount, 0, $"{path} WalkAnimation has no frames");

            Assert.Greater(data.IdleAnimation.FramesPerSecond, 0f, $"{path} IdleAnimation has non-positive FramesPerSecond");
            Assert.Greater(data.WalkAnimation.FramesPerSecond, 0f, $"{path} WalkAnimation has non-positive FramesPerSecond");
        }
    }

    [Test]
    public void SpriteAnimationData_AllAssetsHaveFrames()
    {
        string[] guids = AssetDatabase.FindAssets("t:SpriteAnimationData");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SpriteAnimationData data = AssetDatabase.LoadAssetAtPath<SpriteAnimationData>(path);

            foreach (Sprite frame in data.Frames)
            {
                Assert.IsTrue(frame != null, $"{path} has a null frame");
            }
        }
    }

    [Test]
    public void Prefabs_EntitiesHaveNoAnimator()
    {
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Objects" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab.GetComponent<PlaceableEntity>() == null)
            {
                continue;
            }

            Assert.IsTrue(prefab.GetComponent<Animator>() == null, $"{path} still has an Animator component");
        }
    }

    [Test]
    public void Prefabs_ChickAndPlayerHaveConfigAssigned()
    {
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            foreach (Chick chick in prefab.GetComponentsInChildren<Chick>(true))
            {
                SerializedProperty mutation = new SerializedObject(chick).FindProperty("mutationData");
                Assert.IsTrue(mutation.objectReferenceValue != null, $"{path} Chick has no mutationData assigned");
            }

            foreach (PlayerMovement movement in prefab.GetComponentsInChildren<PlayerMovement>(true))
            {
                SerializedProperty data = new SerializedObject(movement).FindProperty("data");
                Assert.IsTrue(data.objectReferenceValue != null, $"{path} PlayerMovement has no data assigned");
            }
        }
    }
}

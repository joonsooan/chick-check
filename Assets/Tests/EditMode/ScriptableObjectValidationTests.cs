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
            if (data.CanGrow)
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
    public void ChickenData_AllAssetsHaveEggPrefab()
    {
        string[] guids = AssetDatabase.FindAssets("t:ChickenData");
        Assert.IsNotEmpty(guids, "No ChickenData assets found.");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ChickenData data = AssetDatabase.LoadAssetAtPath<ChickenData>(path);
            Assert.IsTrue(data.EggPrefab != null, $"{path} has no EggPrefab assigned");
        }
    }

    [Test]
    public void MutationData_AllAssetsHaveMutatedChickPrefab()
    {
        string[] guids = AssetDatabase.FindAssets("t:MutationData");
        Assert.IsNotEmpty(guids, "No MutationData assets found.");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            MutationData data = AssetDatabase.LoadAssetAtPath<MutationData>(path);
            Assert.IsTrue(data.MutatedChickPrefab != null, $"{path} has no MutatedChickPrefab assigned");
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
    public void DebtConfigData_MultiplierIsAtLeastOne()
    {
        string[] guids = AssetDatabase.FindAssets("t:DebtConfigData");
        Assert.IsNotEmpty(guids, "No DebtConfigData assets found.");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            DebtConfigData data = AssetDatabase.LoadAssetAtPath<DebtConfigData>(path);
            Assert.GreaterOrEqual(data.Multiplier, 1f, $"{path} has Multiplier below 1");
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
            Assert.Greater(item.PriceMultiplier, 0f, $"{path} has non-positive PriceMultiplier");
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

using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;

[TestFixture]
public class ScriptableObjectValidationTests
{
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
                Assert.IsNotNull(
                    data.NextStagePrefab.GetComponent<PlaceableEntity>(),
                    $"{path}.NextStagePrefab is missing a PlaceableEntity component");
            }
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
            Assert.IsNotNull(item.Effect, $"{path} has no Effect assigned");
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
}

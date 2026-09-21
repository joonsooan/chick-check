using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class MigrateShopItemsToEffects
{
    [MenuItem("Chick Colony/Migrations/Shop Items → ShopEffect")]
    public static void Run()
    {
        int migrated = 0;
        int skipped = 0;

        foreach (string guid in AssetDatabase.FindAssets("t:ShopItemData"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ShopItemData item = AssetDatabase.LoadAssetAtPath<ShopItemData>(path);
            if (item == null)
            {
                continue;
            }

            SerializedObject serializedObject = new SerializedObject(item);
            SerializedProperty effectProperty = serializedObject.FindProperty("effect");

            if (effectProperty.managedReferenceValue != null)
            {
                Debug.Log($"Skipping {path} — effect already set.");
                skipped++;
                continue;
            }

            string[] lines = File.ReadAllLines(path);
            ShopEffect effect = BuildEffect(lines);

            if (effect == null)
            {
                Debug.LogWarning($"Could not determine effectKind for {path}.");
                continue;
            }

            effectProperty.managedReferenceValue = effect;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(item);
            migrated++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Shop item migration complete. Migrated: {migrated}, skipped: {skipped}.");
    }

    private static ShopEffect BuildEffect(string[] lines)
    {
        if (!TryReadInt(lines, "effectKind", out int effectKind))
        {
            return null;
        }

        switch (effectKind)
        {
            case 0:
                return BuildStatModifierEffect(lines);
            case 1:
                return new EnableMutationEffect();
            case 2:
                return new EnableContagionEffect();
            case 3:
                return BuildMirrorUpgradeEffect(lines);
            case 4:
                return new EnableSprintEffect();
            case 5:
                return BuildGoldMineEffect(lines);
            case 6:
                return BuildMultiLayEffect(lines);
            default:
                return null;
        }
    }

    private static ShopEffect BuildStatModifierEffect(string[] lines)
    {
        StatModifierEffect effect = new StatModifierEffect();
        SetPrivateField(effect, "targetStat", (ShopStatType)ReadInt(lines, "targetStat"));
        SetPrivateField(effect, "op", (ShopModifierOp)ReadInt(lines, "op"));
        SetPrivateField(effect, "value", ReadFloat(lines, "value"));
        return effect;
    }

    private static ShopEffect BuildMirrorUpgradeEffect(string[] lines)
    {
        GameObject prefab = ReadObjectReference(lines, "mirrorReplacementPrefab");
        MirrorUpgradeEffect effect = new MirrorUpgradeEffect();
        SetPrivateField(effect, "replacementPrefab", prefab);
        return effect;
    }

    private static ShopEffect BuildGoldMineEffect(string[] lines)
    {
        GoldMineEffect effect = new GoldMineEffect();
        SetPrivateField(effect, "interval", ReadFloat(lines, "goldMineInterval"));
        SetPrivateField(effect, "chance", ReadFloat(lines, "goldMineChance"));
        SetPrivateField(effect, "baseGold", ReadInt(lines, "goldMineBaseGold"));
        SetPrivateField(effect, "goldPerPurchase", ReadInt(lines, "goldMineGoldPerPurchase"));
        return effect;
    }

    private static ShopEffect BuildMultiLayEffect(string[] lines)
    {
        MultiLayEffect effect = new MultiLayEffect();
        SetPrivateField(effect, "chance", ReadFloat(lines, "multiLayChance"));
        SetPrivateField(effect, "countPerPurchase", ReadInt(lines, "multiLayCountPerPurchase"));
        return effect;
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(
            fieldName,
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (field == null)
        {
            throw new System.MissingFieldException(target.GetType().Name, fieldName);
        }

        field.SetValue(target, value);
    }

    private static bool TryReadInt(string[] lines, string key, out int value)
    {
        Match match = FindMatch(lines, key, @"(-?\d+)");
        if (match == null)
        {
            value = 0;
            return false;
        }

        value = int.Parse(match.Groups[1].Value);
        return true;
    }

    private static int ReadInt(string[] lines, string key)
    {
        if (!TryReadInt(lines, key, out int value))
        {
            throw new System.FormatException($"Missing legacy key '{key}'.");
        }

        return value;
    }

    private static float ReadFloat(string[] lines, string key)
    {
        Match match = FindMatch(lines, key, @"(-?[\d.]+(?:[eE][-+]?\d+)?)");
        if (match == null)
        {
            throw new System.FormatException($"Missing legacy key '{key}'.");
        }

        return float.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
    }

    private static GameObject ReadObjectReference(string[] lines, string key)
    {
        Match match = FindMatch(lines, key, @"\{fileID: (-?\d+), guid: ([0-9a-fA-F]+), type: \d+\}");
        if (match == null || !match.Success)
        {
            return null;
        }

        long fileId = long.Parse(match.Groups[1].Value);
        if (fileId == 0)
        {
            return null;
        }

        string assetGuid = match.Groups[2].Value;
        string assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
        return string.IsNullOrEmpty(assetPath) ? null : AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
    }

    private static Match FindMatch(string[] lines, string key, string valuePattern)
    {
        Regex regex = new Regex($@"^\s*{Regex.Escape(key)}:\s*{valuePattern}\s*$");
        foreach (string line in lines)
        {
            Match match = regex.Match(line);
            if (match.Success)
            {
                return match;
            }
        }

        return null;
    }
}

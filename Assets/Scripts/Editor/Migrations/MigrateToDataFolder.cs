using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class MigrateToDataFolder
{
    private const string DataRoot = "Assets/Data";
    private const string ConfigFolder = "Assets/Data/Config";
    private const string EntitiesFolder = "Assets/Data/Entities";
    private const string ShopFolder = "Assets/Data/Shop";
    private const string ShopItemsFolder = "Assets/Data/Shop/Items";
    private const string AudioFolder = "Assets/Data/Audio";
    private const string TestDataFolder = "Assets/Data/TestData";

    private const string ResourcesRoot = "Assets/Resources";

    private static readonly string[] ScenePaths =
    {
        "Assets/Scenes/GameScene.unity",
        "Assets/Scenes/TutorialScene.unity",
    };

    [MenuItem("Chick Colony/Migrations/Split Config And Move To Data")]
    public static void Run()
    {
        if (!CreateFolders())
        {
            return;
        }

        if (!MoveExistingAssets())
        {
            return;
        }

        if (!CreateNewConfigAssets())
        {
            return;
        }

        if (!FixExplicitValues())
        {
            return;
        }

        if (!ReconnectScenes())
        {
            return;
        }

        if (!ReconnectPrefabs())
        {
            return;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("MigrateToDataFolder: all steps completed successfully.");
    }

    private static bool CreateFolders()
    {
        Debug.Log("MigrateToDataFolder: step 1 — creating folders.");

        EnsureFolder("Assets", "Data");
        EnsureFolder(DataRoot, "Config");
        EnsureFolder(DataRoot, "Entities");
        EnsureFolder(DataRoot, "Shop");
        EnsureFolder(ShopFolder, "Items");
        EnsureFolder(DataRoot, "Audio");
        EnsureFolder(DataRoot, "TestData");

        return true;
    }

    private static void EnsureFolder(string parent, string name)
    {
        string path = $"{parent}/{name}";
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, name);
        }
    }

    private static bool MoveExistingAssets()
    {
        Debug.Log("MigrateToDataFolder: step 2 — moving existing assets.");

        if (!MoveAsset($"{ResourcesRoot}/RoundConfigData.asset", $"{ConfigFolder}/RoundConfig.asset"))
        {
            return false;
        }

        foreach (string name in new[] { "PlayerCarryData", "MirrorData", "MutationData" })
        {
            if (!MoveAsset($"{ResourcesRoot}/{name}.asset", $"{ConfigFolder}/{name}.asset"))
            {
                return false;
            }
        }

        foreach (string name in new[] { "ChickData", "ChickData_Mutant", "ChickenData", "ChickenData_Mutant", "EggData" })
        {
            if (!MoveAsset($"{ResourcesRoot}/{name}.asset", $"{EntitiesFolder}/{name}.asset"))
            {
                return false;
            }
        }

        if (!MoveShopItems())
        {
            return false;
        }

        if (!MoveAsset($"{ResourcesRoot}/Sound/AudioLibrary.asset", $"{AudioFolder}/AudioLibrary.asset"))
        {
            return false;
        }

        if (!MoveAudioClipFolders())
        {
            return false;
        }

        if (!MoveTestData())
        {
            return false;
        }

        DeleteResourcesFolderIfEmpty();

        return true;
    }

    private static bool MoveShopItems()
    {
        string sourceFolder = $"{ResourcesRoot}/Shop Items";
        if (!AssetDatabase.IsValidFolder(sourceFolder))
        {
            return true;
        }

        string[] guids = AssetDatabase.FindAssets("t:ShopItemData", new[] { sourceFolder });
        foreach (string guid in guids)
        {
            string sourcePath = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileName(sourcePath);
            string destinationPath = $"{ShopItemsFolder}/{fileName}";

            if (!MoveAsset(sourcePath, destinationPath))
            {
                return false;
            }
        }

        DeleteFolderIfEmpty(sourceFolder);

        return true;
    }

    private static bool MoveAudioClipFolders()
    {
        EnsureFolder("Assets", "Audio");

        if (!MoveFolder($"{ResourcesRoot}/Sound/BGM", "Assets/Audio/BGM"))
        {
            return false;
        }

        if (!MoveFolder($"{ResourcesRoot}/Sound/SFX", "Assets/Audio/SFX"))
        {
            return false;
        }

        DeleteFolderIfEmpty($"{ResourcesRoot}/Sound");

        return true;
    }

    private static bool MoveTestData()
    {
        string sourceFolder = $"{ResourcesRoot}/Test Data";
        if (!AssetDatabase.IsValidFolder(sourceFolder))
        {
            return true;
        }

        string[] guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { sourceFolder });
        foreach (string guid in guids)
        {
            string sourcePath = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileName(sourcePath);
            string destinationPath = $"{TestDataFolder}/{fileName}";

            if (!MoveAsset(sourcePath, destinationPath))
            {
                return false;
            }
        }

        DeleteFolderIfEmpty(sourceFolder);

        return true;
    }

    private static void DeleteResourcesFolderIfEmpty()
    {
        if (!AssetDatabase.IsValidFolder(ResourcesRoot))
        {
            return;
        }

        string[] remaining = AssetDatabase.FindAssets(string.Empty, new[] { ResourcesRoot });
        if (remaining.Length == 0)
        {
            AssetDatabase.DeleteAsset(ResourcesRoot);
            Debug.Log("MigrateToDataFolder: deleted empty Assets/Resources folder.");
        }
        else
        {
            Debug.LogWarning($"MigrateToDataFolder: Assets/Resources still has {remaining.Length} asset(s), leaving it in place.");
        }
    }

    private static bool MoveAsset(string sourcePath, string destinationPath)
    {
        if (AssetDatabase.LoadMainAssetAtPath(destinationPath) != null)
        {
            return true;
        }

        if (AssetDatabase.LoadMainAssetAtPath(sourcePath) == null)
        {
            Debug.LogWarning($"MigrateToDataFolder: source asset not found, skipping: {sourcePath}");
            return true;
        }

        string error = AssetDatabase.MoveAsset(sourcePath, destinationPath);
        if (!string.IsNullOrEmpty(error))
        {
            Debug.LogError($"MigrateToDataFolder: failed to move {sourcePath} -> {destinationPath}: {error}");
            return false;
        }

        return true;
    }

    private static bool MoveFolder(string sourcePath, string destinationPath)
    {
        if (AssetDatabase.IsValidFolder(destinationPath))
        {
            return true;
        }

        if (!AssetDatabase.IsValidFolder(sourcePath))
        {
            Debug.LogWarning($"MigrateToDataFolder: source folder not found, skipping: {sourcePath}");
            return true;
        }

        string error = AssetDatabase.MoveAsset(sourcePath, destinationPath);
        if (!string.IsNullOrEmpty(error))
        {
            Debug.LogError($"MigrateToDataFolder: failed to move folder {sourcePath} -> {destinationPath}: {error}");
            return false;
        }

        return true;
    }

    private static void DeleteFolderIfEmpty(string folderPath)
    {
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            return;
        }

        string[] remaining = AssetDatabase.FindAssets(string.Empty, new[] { folderPath });
        if (remaining.Length == 0)
        {
            AssetDatabase.DeleteAsset(folderPath);
        }
    }

    private static bool CreateNewConfigAssets()
    {
        Debug.Log("MigrateToDataFolder: step 3 — creating new config assets.");

        CreateAssetIfMissing<DebtConfigData>($"{ConfigFolder}/DebtConfig.asset");
        CreateAssetIfMissing<EconomyConfigData>($"{ConfigFolder}/EconomyConfig.asset");
        CreateAssetIfMissing<DayNightConfigData>($"{ConfigFolder}/DayNightConfig.asset");
        CreateAssetIfMissing<PlayerData>($"{ConfigFolder}/PlayerData.asset");

        CreateShopConfigIfMissing();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return true;
    }

    private static void CreateAssetIfMissing<T>(string path) where T : ScriptableObject
    {
        if (AssetDatabase.LoadAssetAtPath<T>(path) != null)
        {
            return;
        }

        T instance = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(instance, path);
        Debug.Log($"MigrateToDataFolder: created {path}");
    }

    private static void CreateShopConfigIfMissing()
    {
        string path = $"{ShopFolder}/ShopConfig.asset";
        if (AssetDatabase.LoadAssetAtPath<ShopConfigData>(path) != null)
        {
            return;
        }

        ShopConfigData config = ScriptableObject.CreateInstance<ShopConfigData>();

        List<ShopItemData> catalog = AssetDatabase.FindAssets("t:ShopItemData", new[] { ShopItemsFolder })
            .Select(AssetDatabase.GUIDToAssetPath)
            .OrderBy(p => p, System.StringComparer.Ordinal)
            .Select(AssetDatabase.LoadAssetAtPath<ShopItemData>)
            .Where(item => item != null)
            .ToList();

        AssetDatabase.CreateAsset(config, path);

        SerializedObject serializedConfig = new SerializedObject(config);
        SerializedProperty catalogProperty = serializedConfig.FindProperty("catalog");
        catalogProperty.arraySize = catalog.Count;
        for (int i = 0; i < catalog.Count; i++)
        {
            catalogProperty.GetArrayElementAtIndex(i).objectReferenceValue = catalog[i];
        }

        serializedConfig.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(config);

        Debug.Log($"MigrateToDataFolder: created {path} with {catalog.Count} catalog entries.");
    }

    private static bool FixExplicitValues()
    {
        Debug.Log("MigrateToDataFolder: step 4 — fixing explicit values.");

        MirrorData mirrorData = AssetDatabase.LoadAssetAtPath<MirrorData>($"{ConfigFolder}/MirrorData.asset");
        if (mirrorData != null)
        {
            SerializedObject serializedMirror = new SerializedObject(mirrorData);
            serializedMirror.FindProperty("mutantExitSpeedMultiplier").floatValue = 1.5f;
            serializedMirror.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(mirrorData);
        }
        else
        {
            Debug.LogWarning($"MigrateToDataFolder: MirrorData.asset not found at {ConfigFolder}, skipping explicit fix.");
        }

        EggData eggData = AssetDatabase.LoadAssetAtPath<EggData>($"{EntitiesFolder}/EggData.asset");
        if (eggData != null)
        {
            SerializedObject serializedEgg = new SerializedObject(eggData);
            serializedEgg.FindProperty("displayOrder").intValue = 0;
            serializedEgg.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(eggData);
        }
        else
        {
            Debug.LogWarning($"MigrateToDataFolder: EggData.asset not found at {EntitiesFolder}, skipping explicit fix.");
        }

        AssetDatabase.SaveAssets();

        return true;
    }

    private static bool ReconnectScenes()
    {
        Debug.Log("MigrateToDataFolder: step 5 — reconnecting scenes.");

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            Debug.LogError("MigrateToDataFolder: aborted because the current scene was not saved.");
            return false;
        }

        string originalScenePath = EditorSceneManager.GetActiveScene().path;

        bool success = true;

        foreach (string scenePath in ScenePaths)
        {
            if (!File.Exists(scenePath))
            {
                Debug.LogWarning($"MigrateToDataFolder: scene not found, skipping: {scenePath}");
                continue;
            }

            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            DebtConfigData debtConfig = AssetDatabase.LoadAssetAtPath<DebtConfigData>($"{ConfigFolder}/DebtConfig.asset");
            ShopConfigData shopConfig = AssetDatabase.LoadAssetAtPath<ShopConfigData>($"{ShopFolder}/ShopConfig.asset");
            EconomyConfigData economyConfig = AssetDatabase.LoadAssetAtPath<EconomyConfigData>($"{ConfigFolder}/EconomyConfig.asset");
            DayNightConfigData dayNightConfig = AssetDatabase.LoadAssetAtPath<DayNightConfigData>($"{ConfigFolder}/DayNightConfig.asset");

            bool dirty = false;
            dirty |= AssignToAll<GameManager>(scene, "debtConfig", debtConfig);
            dirty |= AssignToAll<UpgradeManager>(scene, "shopConfig", shopConfig);
            dirty |= AssignToAll<EconomyManager>(scene, "economyConfig", economyConfig);
            dirty |= AssignToAll<DayNightController>(scene, "dayNightConfig", dayNightConfig);

            if (dirty)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene))
                {
                    Debug.LogError($"MigrateToDataFolder: failed to save scene {scenePath}");
                    success = false;
                }
            }
        }

        if (!string.IsNullOrEmpty(originalScenePath))
        {
            EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
        }

        return success;
    }

    private static bool AssignToAll<T>(UnityEngine.SceneManagement.Scene scene, string fieldName, ScriptableObject value)
        where T : MonoBehaviour
    {
        if (value == null)
        {
            Debug.LogError($"MigrateToDataFolder: {typeof(T).Name}.{fieldName} target asset is null in {scene.path}.");
            return false;
        }

        T[] instances = Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        bool changed = false;

        foreach (T instance in instances)
        {
            SerializedObject serializedObject = new SerializedObject(instance);
            SerializedProperty property = serializedObject.FindProperty(fieldName);
            if (property == null)
            {
                Debug.LogError($"MigrateToDataFolder: field '{fieldName}' not found on {typeof(T).Name} in {scene.path}");
                continue;
            }

            if (property.objectReferenceValue == value)
            {
                continue;
            }

            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            changed = true;
        }

        return changed;
    }

    private static bool ReconnectPrefabs()
    {
        Debug.Log("MigrateToDataFolder: step 6 — reconnecting prefabs.");

        PlayerData playerData = AssetDatabase.LoadAssetAtPath<PlayerData>($"{ConfigFolder}/PlayerData.asset");
        MutationData mutationData = AssetDatabase.LoadAssetAtPath<MutationData>($"{ConfigFolder}/MutationData.asset");

        if (playerData == null || mutationData == null)
        {
            Debug.LogError("MigrateToDataFolder: PlayerData or MutationData asset missing, aborting prefab reconnection.");
            return false;
        }

        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" });

        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            bool dirty = false;

            try
            {
                PlayerMovement playerMovement = root.GetComponentInChildren<PlayerMovement>(true);
                if (playerMovement != null)
                {
                    dirty |= SetFieldIfDifferent(playerMovement, "data", playerData);
                }

                Chick[] chicks = root.GetComponentsInChildren<Chick>(true);
                foreach (Chick chick in chicks)
                {
                    dirty |= SetFieldIfDifferent(chick, "mutationData", mutationData);
                }

                if (dirty)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    Debug.Log($"MigrateToDataFolder: updated prefab {path}");
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        return true;
    }

    private static bool SetFieldIfDifferent(Object target, string fieldName, ScriptableObject value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(fieldName);
        if (property == null)
        {
            Debug.LogError($"MigrateToDataFolder: field '{fieldName}' not found on {target.GetType().Name}");
            return false;
        }

        if (property.objectReferenceValue == value)
        {
            return false;
        }

        property.objectReferenceValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        return true;
    }
}

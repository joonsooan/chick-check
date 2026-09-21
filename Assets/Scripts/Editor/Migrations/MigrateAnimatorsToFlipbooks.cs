using System.Linq;
using UnityEditor;
using UnityEngine;

public static class MigrateAnimatorsToFlipbooks
{
    private const string AnimationsFolder = "Assets/Data/Animations";
    private const string PrefabsFolder = "Assets/Prefabs/Objects";

    [MenuItem("Chick Colony/Migrations/Animators To Flipbooks")]
    public static void Run()
    {
        EnsureAnimationsFolder();

        int prefabCount = 0;
        int clipCount = 0;

        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { PrefabsFolder });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject root = null;

            try
            {
                root = PrefabUtility.LoadPrefabContents(path);

                Animator animator = root.GetComponent<Animator>();
                PlaceableEntity placeableEntity = root.GetComponent<PlaceableEntity>();

                if (animator == null || placeableEntity == null)
                {
                    continue;
                }

                SpriteAnimationData idleAnimation = null;
                SpriteAnimationData walkAnimation = null;

                RuntimeAnimatorController controller = animator.runtimeAnimatorController;
                if (controller != null)
                {
                    foreach (AnimationClip clip in controller.animationClips)
                    {
                        if (clip == null)
                        {
                            continue;
                        }

                        SpriteAnimationData animationData = ConvertClip(clip);
                        if (animationData == null)
                        {
                            continue;
                        }

                        clipCount++;

                        if (clip.name.EndsWith("Idle"))
                        {
                            idleAnimation = animationData;
                        }
                        else if (clip.name.EndsWith("Walk"))
                        {
                            walkAnimation = animationData;
                        }
                    }
                }

                SerializedObject entitySerialized = new SerializedObject(placeableEntity);
                SerializedProperty dataProperty = entitySerialized.FindProperty("data");
                EconomyEntityData economyData = dataProperty != null ? dataProperty.objectReferenceValue as EconomyEntityData : null;

                if (economyData != null)
                {
                    SerializedObject dataSerialized = new SerializedObject(economyData);
                    SerializedProperty idleProperty = dataSerialized.FindProperty("idleAnimation");
                    SerializedProperty walkProperty = dataSerialized.FindProperty("walkAnimation");

                    if (idleProperty != null)
                    {
                        idleProperty.objectReferenceValue = idleAnimation;
                    }

                    if (walkProperty != null)
                    {
                        walkProperty.objectReferenceValue = walkAnimation;
                    }

                    dataSerialized.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(economyData);
                }

                Object.DestroyImmediate(animator, true);
                PrefabUtility.SaveAsPrefabAsset(root, path);
                prefabCount++;
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"Failed to migrate prefab at {path}: {exception}");
            }
            finally
            {
                if (root != null)
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Migrated {prefabCount} prefabs, converted {clipCount} clips into SpriteAnimationData assets.");
    }

    private static void EnsureAnimationsFolder()
    {
        if (!AssetDatabase.IsValidFolder(AnimationsFolder))
        {
            AssetDatabase.CreateFolder("Assets/Data", "Animations");
        }
    }

    private static SpriteAnimationData ConvertClip(AnimationClip clip)
    {
        EditorCurveBinding[] bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
        EditorCurveBinding spriteBinding = default;
        bool found = false;

        foreach (EditorCurveBinding binding in bindings)
        {
            if (binding.propertyName == "m_Sprite")
            {
                spriteBinding = binding;
                found = true;
                break;
            }
        }

        if (!found)
        {
            return null;
        }

        ObjectReferenceKeyframe[] keyframes = AnimationUtility.GetObjectReferenceCurve(clip, spriteBinding);
        Sprite[] frames = keyframes
            .OrderBy(keyframe => keyframe.time)
            .Select(keyframe => keyframe.value as Sprite)
            .ToArray();

        bool loop = AnimationUtility.GetAnimationClipSettings(clip).loopTime;

        string assetPath = $"{AnimationsFolder}/{clip.name}.asset";
        SpriteAnimationData animationData = AssetDatabase.LoadAssetAtPath<SpriteAnimationData>(assetPath);
        bool isNew = animationData == null;

        if (isNew)
        {
            animationData = ScriptableObject.CreateInstance<SpriteAnimationData>();
        }

        SerializedObject serializedAnimation = new SerializedObject(animationData);
        SerializedProperty framesProperty = serializedAnimation.FindProperty("frames");
        framesProperty.arraySize = frames.Length;
        for (int i = 0; i < frames.Length; i++)
        {
            framesProperty.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
        }

        serializedAnimation.FindProperty("framesPerSecond").floatValue = clip.frameRate;
        serializedAnimation.FindProperty("loop").boolValue = loop;
        serializedAnimation.ApplyModifiedPropertiesWithoutUndo();

        if (isNew)
        {
            AssetDatabase.CreateAsset(animationData, assetPath);
        }
        else
        {
            EditorUtility.SetDirty(animationData);
        }

        return animationData;
    }
}

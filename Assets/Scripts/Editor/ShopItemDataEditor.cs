using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ShopItemData))]
public class ShopItemDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty effectKind = serializedObject.FindProperty("effectKind");

        DrawProperty("itemName");
        DrawProperty("description");
        DrawIcon();
        DrawProperty("cost");
        DrawProperty("priceMultiplier");
        EditorGUILayout.PropertyField(effectKind);
        DrawProperty("oneTimeOnly");

        ShopEffectKind kind = (ShopEffectKind)effectKind.enumValueIndex;

        switch (kind)
        {
            case ShopEffectKind.StatModifier:
                DrawSection("Stat Modifier", "targetStat", "op", "value");
                break;
            case ShopEffectKind.MirrorUpgrade:
                DrawSection("Mirror Upgrade", "mirrorReplacementPrefab");
                break;
            case ShopEffectKind.GoldMine:
                DrawSection("Gold Mine", "goldMineInterval", "goldMineChance", "goldMineBaseGold", "goldMineGoldPerPurchase");
                break;
            case ShopEffectKind.MultiLay:
                DrawSection("Multi Lay", "multiLayChance", "multiLayCountPerPurchase");
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawSection(string header, params string[] propertyNames)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(header, EditorStyles.boldLabel);
        foreach (string name in propertyNames)
        {
            DrawProperty(name);
        }
    }

    private void DrawIcon()
    {
        SerializedProperty icon = serializedObject.FindProperty("icon");
        icon.objectReferenceValue = EditorGUILayout.ObjectField("Icon", icon.objectReferenceValue, typeof(Sprite), false);
    }

    private void DrawProperty(string name)
    {
        EditorGUILayout.PropertyField(serializedObject.FindProperty(name));
    }
}

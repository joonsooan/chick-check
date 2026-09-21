using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ShopEffect), true)]
public class ShopEffectDrawer : PropertyDrawer
{
    private static Type[] effectTypes;
    private static string[] effectTypeNames;

    private static void EnsureTypeCache()
    {
        if (effectTypes != null)
        {
            return;
        }

        effectTypes = TypeCache.GetTypesDerivedFrom<ShopEffect>()
            .Where(type => !type.IsAbstract)
            .OrderBy(type => type.Name)
            .ToArray();

        effectTypeNames = new[] { "(None)" }.Concat(effectTypes.Select(type => type.Name)).ToArray();
    }

    private static int CurrentTypeIndex(SerializedProperty property)
    {
        object value = property.managedReferenceValue;
        if (value == null)
        {
            return 0;
        }

        Type valueType = value.GetType();
        for (int i = 0; i < effectTypes.Length; i++)
        {
            if (effectTypes[i] == valueType)
            {
                return i + 1;
            }
        }

        return 0;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EnsureTypeCache();

        Rect popupRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        int currentIndex = CurrentTypeIndex(property);

        EditorGUI.BeginProperty(position, label, property);

        int selectedIndex = EditorGUI.Popup(popupRect, label.text, currentIndex, effectTypeNames);
        if (selectedIndex != currentIndex)
        {
            property.managedReferenceValue = selectedIndex == 0
                ? null
                : Activator.CreateInstance(effectTypes[selectedIndex - 1]);
        }

        if (property.managedReferenceValue != null)
        {
            float y = popupRect.yMax + EditorGUIUtility.standardVerticalSpacing;
            SerializedProperty child = property.Copy();
            SerializedProperty end = property.GetEndProperty();
            bool enterChildren = true;

            while (child.NextVisible(enterChildren) && !SerializedProperty.EqualContents(child, end))
            {
                enterChildren = false;
                float height = EditorGUI.GetPropertyHeight(child, true);
                Rect childRect = new Rect(position.x, y, position.width, height);
                EditorGUI.PropertyField(childRect, child, true);
                y += height + EditorGUIUtility.standardVerticalSpacing;
            }
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;

        if (property.managedReferenceValue != null)
        {
            SerializedProperty child = property.Copy();
            SerializedProperty end = property.GetEndProperty();
            bool enterChildren = true;

            while (child.NextVisible(enterChildren) && !SerializedProperty.EqualContents(child, end))
            {
                enterChildren = false;
                height += EditorGUIUtility.standardVerticalSpacing;
                height += EditorGUI.GetPropertyHeight(child, true);
            }
        }

        return height;
    }
}

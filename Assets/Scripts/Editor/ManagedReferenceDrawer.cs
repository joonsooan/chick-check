using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ShopEffect), true)]
[CustomPropertyDrawer(typeof(TutorialCondition), true)]
public class ManagedReferenceDrawer : PropertyDrawer
{
    private class TypeCatalog
    {
        public Type[] Types;
        public string[] DisplayNames;
    }

    private static readonly Dictionary<Type, TypeCatalog> catalogs = new Dictionary<Type, TypeCatalog>();

    private static Type ElementType(Type fieldType)
    {
        if (fieldType.IsArray)
        {
            return fieldType.GetElementType();
        }

        if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
        {
            return fieldType.GetGenericArguments()[0];
        }

        return fieldType;
    }

    private TypeCatalog Catalog()
    {
        Type baseType = ElementType(fieldInfo.FieldType);
        if (catalogs.TryGetValue(baseType, out TypeCatalog catalog))
        {
            return catalog;
        }

        Type[] types = TypeCache.GetTypesDerivedFrom(baseType)
            .Where(type => !type.IsAbstract)
            .OrderBy(type => type.Name)
            .ToArray();

        catalog = new TypeCatalog
        {
            Types = types,
            DisplayNames = new[] { "(None)" }.Concat(types.Select(type => type.Name)).ToArray(),
        };
        catalogs[baseType] = catalog;
        return catalog;
    }

    private static int CurrentTypeIndex(SerializedProperty property, TypeCatalog catalog)
    {
        object value = property.managedReferenceValue;
        if (value == null)
        {
            return 0;
        }

        int index = Array.IndexOf(catalog.Types, value.GetType());
        return index < 0 ? 0 : index + 1;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        TypeCatalog catalog = Catalog();

        Rect popupRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        int currentIndex = CurrentTypeIndex(property, catalog);

        EditorGUI.BeginProperty(position, label, property);

        int selectedIndex = EditorGUI.Popup(popupRect, label.text, currentIndex, catalog.DisplayNames);
        if (selectedIndex != currentIndex)
        {
            property.managedReferenceValue = selectedIndex == 0
                ? null
                : Activator.CreateInstance(catalog.Types[selectedIndex - 1]);
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

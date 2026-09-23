using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ShopItemData))]
public class ShopItemDataEditor : Editor
{
    private const float PreviewSize = 128f;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Sprite icon = serializedObject.FindProperty("icon").objectReferenceValue as Sprite;
        if (icon == null || icon.texture == null)
        {
            return;
        }

        EditorGUILayout.Space();
        Rect rect = GUILayoutUtility.GetRect(PreviewSize, PreviewSize, GUILayout.ExpandWidth(false));
        rect.x = (EditorGUIUtility.currentViewWidth - PreviewSize) * 0.5f;
        EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.2f));
        DrawSpriteFitted(rect, icon);
    }

    private static void DrawSpriteFitted(Rect rect, Sprite sprite)
    {
        Rect textureRect = sprite.textureRect;
        float aspect = textureRect.width / textureRect.height;
        Rect fitted = rect;
        if (aspect >= 1f)
        {
            fitted.height = rect.width / aspect;
            fitted.y += (rect.height - fitted.height) * 0.5f;
        }
        else
        {
            fitted.width = rect.height * aspect;
            fitted.x += (rect.width - fitted.width) * 0.5f;
        }

        Texture2D texture = sprite.texture;
        Rect texCoords = new Rect(
            textureRect.x / texture.width,
            textureRect.y / texture.height,
            textureRect.width / texture.width,
            textureRect.height / texture.height);

        FilterMode previousFilter = texture.filterMode;
        texture.filterMode = FilterMode.Point;
        GUI.DrawTextureWithTexCoords(fitted, texture, texCoords, true);
        texture.filterMode = previousFilter;
    }
}

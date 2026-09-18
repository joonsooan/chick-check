using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ShopItemData))]
public class ShopItemDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Sprite icon = ((ShopItemData)target).Icon;
        if (icon == null)
        {
            return;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Icon Preview", EditorStyles.boldLabel);

        Rect rect = GUILayoutUtility.GetRect(64, 64, GUILayout.ExpandWidth(false));
        Texture2D tex = icon.texture;
        Rect uv = icon.textureRect;
        uv.x /= tex.width;
        uv.y /= tex.height;
        uv.width /= tex.width;
        uv.height /= tex.height;
        GUI.DrawTextureWithTexCoords(rect, tex, uv);
    }
}

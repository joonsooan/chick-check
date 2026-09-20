using UnityEngine;
using UnityEngine.UI;

public static class UILayoutUtil
{
    private static readonly System.Collections.Generic.List<LayoutGroup> groupBuffer =
        new System.Collections.Generic.List<LayoutGroup>();

    public static void Rebuild(RectTransform root)
    {
        if (root == null || !root.gameObject.activeInHierarchy)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();

        root.GetComponentsInChildren(true, groupBuffer);
        for (int i = groupBuffer.Count - 1; i >= 0; i--)
        {
            LayoutGroup group = groupBuffer[i];
            if (group != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)group.transform);
            }
        }
        groupBuffer.Clear();

        LayoutRebuilder.ForceRebuildLayoutImmediate(root);
    }
}

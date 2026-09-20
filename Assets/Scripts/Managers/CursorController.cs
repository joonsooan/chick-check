using UnityEngine;
using UnityEngine.SceneManagement;

public class CursorController : MonoBehaviour
{
    private static CursorController instance;

    [SerializeField] private Texture2D cursorTexture;
    [SerializeField] private Vector2 hotspot = Vector2.zero;
    [SerializeField, Min(1)] private int scale = 2;

    private Texture2D scaledTexture;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        if (scale > 1)
        {
            BuildScaledTexture();
        }

        ApplyCursor();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Unity resets the software cursor on scene load, so re-apply each time.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyCursor();
    }

    private void ApplyCursor()
    {
        if (cursorTexture == null)
        {
            return;
        }

        Texture2D tex = scaledTexture != null ? scaledTexture : cursorTexture;
        Cursor.SetCursor(tex, hotspot * scale, CursorMode.ForceSoftware);
    }

    // Point-sample integer upscale so pixel-art cursor stays crisp when enlarged.
    private void BuildScaledTexture()
    {
        int width = cursorTexture.width * scale;
        int height = cursorTexture.height * scale;
        scaledTexture = new Texture2D(width, height, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point
        };

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                scaledTexture.SetPixel(x, y, cursorTexture.GetPixel(x / scale, y / scale));
            }
        }

        scaledTexture.Apply();
    }

    private void OnDestroy()
    {
        if (instance != this)
        {
            return;
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (scaledTexture != null)
        {
            Destroy(scaledTexture);
        }
    }
}

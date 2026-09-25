using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CursorController : MonoBehaviour
{
    private static CursorController instance;

    [SerializeField] private Texture2D cursorTexture;
    [SerializeField] private Vector2 hotspot = Vector2.zero;
    [SerializeField, Min(1)] private int scale = 2;
    [SerializeField, Min(0.1f)] private float idleHideDelay = 3f;

    private Texture2D scaledTexture;
    private float idleTime;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        if (scale > 1 && cursorTexture != null)
        {
            BuildScaledTexture();
        }

        ApplyCursor();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyCursor();
        ShowCursor();
    }

    private void Update()
    {
        if (!IsDayPlaying() || HasMouseActivity())
        {
            ShowCursor();
            return;
        }

        idleTime += Time.unscaledDeltaTime;
        if (idleTime >= idleHideDelay && Cursor.visible)
        {
            Cursor.visible = false;
        }
    }

    private static bool IsDayPlaying()
    {
        GameManager gameManager = GameManager.Instance;
        return gameManager != null
            && gameManager.CurrentPhase == GamePhase.Playing
            && Time.timeScale > 0f;
    }

    private static bool HasMouseActivity()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null)
        {
            return false;
        }

        return mouse.delta.ReadValue() != Vector2.zero
            || mouse.scroll.ReadValue() != Vector2.zero
            || mouse.leftButton.isPressed
            || mouse.rightButton.isPressed
            || mouse.middleButton.isPressed;
    }

    private void ShowCursor()
    {
        idleTime = 0f;
        if (!Cursor.visible)
        {
            Cursor.visible = true;
        }
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
        Cursor.visible = true;
        if (scaledTexture != null)
        {
            Destroy(scaledTexture);
        }
    }
}

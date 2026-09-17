using UnityEngine;
using UnityEngine.InputSystem;

public class PlacementController : MonoBehaviour
{
    [SerializeField] private Chick chickPrefab;
    [SerializeField] private Camera cam;

    void Update()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
        {
            return;
        }

        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector3 world = cam.ScreenToWorldPoint(screenPos);
        world.z = 0f;

        Chick chick = Instantiate(chickPrefab);
        chick.transform.position = world;
    }
}

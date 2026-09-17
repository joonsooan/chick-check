using UnityEngine;

public abstract class PlaceableEntity : MonoBehaviour
{
    protected SpriteRenderer spriteRenderer;

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
}

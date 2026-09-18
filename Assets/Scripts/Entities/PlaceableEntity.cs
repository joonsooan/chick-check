using UnityEngine;

public abstract class PlaceableEntity : MonoBehaviour
{
    protected SpriteRenderer spriteRenderer;

    public SpriteRenderer SpriteRenderer => spriteRenderer;

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
}

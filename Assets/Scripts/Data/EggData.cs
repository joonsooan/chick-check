using UnityEngine;

[CreateAssetMenu(fileName = "EggData", menuName = "Egg Data")]
public class EggData : EconomyEntityData
{
    public override EntityKind Kind => EntityKind.Egg;

    [Header("Hatch Squash VFX")]
    [SerializeField] private Vector2 stretchScale = new Vector2(0.7f, 1.3f);
    [SerializeField, Min(0)] private float stretchDuration = 0.12f;
    [SerializeField] private Vector2 squashScale = new Vector2(1.3f, 0.55f);
    [SerializeField, Min(0)] private float squashDuration = 0.1f;

    public Vector2 StretchScale => stretchScale;
    public float StretchDuration => stretchDuration;
    public Vector2 SquashScale => squashScale;
    public float SquashDuration => squashDuration;
}

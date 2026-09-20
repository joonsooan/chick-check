using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugStatsPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private Key toggleKey = Key.F1;
    [SerializeField] private bool show = true;

    private readonly StringBuilder builder = new StringBuilder();

    private void Start()
    {
        ApplyVisibility();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard[toggleKey].wasPressedThisFrame)
        {
            show = !show;
            ApplyVisibility();
        }

        if (!show || label == null)
        {
            return;
        }

        builder.Clear();

        var entities = PlaceableEntity.ActiveEntities;
        for (int i = 0; i < entities.Count; i++)
        {
            AppendLine(entities[i]);
        }

        label.text = builder.ToString();
    }

    private void AppendLine(PlaceableEntity entity)
    {
        if (entity == null)
        {
            return;
        }

        builder.Append(entity.GetType().Name);
        builder.Append(": 성장 ");
        builder.Append(entity.CurrentGrowthInterval.ToString("0.0"));
        builder.Append("s / ");
        builder.Append(entity.CurrentGrowthChance.ToString("P0"));

        if (entity is Chicken chicken)
        {
            builder.Append(" | 산란 ");
            builder.Append(chicken.CurrentLayInterval.ToString("0.0"));
            builder.Append("s / ");
            builder.Append(chicken.CurrentLayChance.ToString("P0"));
        }

        builder.Append('\n');
    }

    private void ApplyVisibility()
    {
        if (label != null)
        {
            label.enabled = show;
        }
    }
}

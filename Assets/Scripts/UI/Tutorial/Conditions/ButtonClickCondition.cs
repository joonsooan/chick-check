using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ButtonClickCondition : TutorialCondition
{
    [SerializeField] private Button button;

    private bool clicked;

    public override void Begin(TutorialContext context)
    {
        clicked = false;
        if (button != null)
        {
            button.onClick.AddListener(HandleClick);
        }
    }

    public override bool IsMet(TutorialContext context) => clicked;

    public override void End(TutorialContext context)
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleClick);
        }
    }

    private void HandleClick() => clicked = true;
}

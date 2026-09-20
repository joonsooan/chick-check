using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[AddComponentMenu("UI/Custom Button")]
public class CustomButton : Button
{
    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        if (IsInteractable() && eventData.button == PointerEventData.InputButton.Left)
        {
            AudioManager.Instance?.PlayUiClick();
        }
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        if (!IsInteractable() || IsPressed())
        {
            return;
        }

        DoStateTransition(SelectionState.Highlighted, false);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        if (!IsInteractable())
        {
            return;
        }

        if (IsPressed())
        {
            return;
        }

        if (eventData.pointerEnter == gameObject || eventData.pointerEnter != null && eventData.pointerEnter.transform.IsChildOf(transform))
        {
            DoStateTransition(SelectionState.Highlighted, false);
        }
    }
}

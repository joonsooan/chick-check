using System;

[Serializable]
public class ConfirmClickCondition : TutorialCondition
{
    private bool confirmed;

    public override bool RequiresConfirmButton => true;

    public override void Begin(TutorialContext context)
    {
        confirmed = false;
        if (context.Popup != null)
        {
            context.Popup.OnConfirm += HandleConfirm;
        }
    }

    public override bool IsMet(TutorialContext context) => confirmed;

    public override void End(TutorialContext context)
    {
        if (context.Popup != null)
        {
            context.Popup.OnConfirm -= HandleConfirm;
        }
    }

    private void HandleConfirm() => confirmed = true;
}

using System;

[Serializable]
public class PickUpCondition : TutorialCondition
{
    private bool pickedUp;

    public override void Begin(TutorialContext context)
    {
        pickedUp = false;
        if (context.PlayerCarry != null)
        {
            context.PlayerCarry.OnPickedUp += HandlePickedUp;
        }
    }

    public override bool IsMet(TutorialContext context) => pickedUp;

    public override void End(TutorialContext context)
    {
        if (context.PlayerCarry != null)
        {
            context.PlayerCarry.OnPickedUp -= HandlePickedUp;
        }
    }

    private void HandlePickedUp(PlaceableEntity entity) => pickedUp = true;
}

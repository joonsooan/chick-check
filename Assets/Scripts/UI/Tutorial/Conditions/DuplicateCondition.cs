using System;

[Serializable]
public class DuplicateCondition : TutorialCondition
{
    private bool duplicated;

    public override void Begin(TutorialContext context)
    {
        duplicated = false;
        if (context.Mirror != null)
        {
            context.Mirror.OnDuplicated += HandleDuplicated;
        }
    }

    public override bool IsMet(TutorialContext context) => duplicated;

    public override void End(TutorialContext context)
    {
        if (context.Mirror != null)
        {
            context.Mirror.OnDuplicated -= HandleDuplicated;
        }
    }

    private void HandleDuplicated() => duplicated = true;
}

using System;

[Serializable]
public abstract class TutorialCondition
{
    public virtual bool RequiresConfirmButton => false;

    public virtual void Begin(TutorialContext context) { }
    public abstract bool IsMet(TutorialContext context);
    public virtual void End(TutorialContext context) { }
}

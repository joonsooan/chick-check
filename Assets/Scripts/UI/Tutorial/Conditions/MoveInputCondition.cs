using System;
using UnityEngine;

[Serializable]
public class MoveInputCondition : TutorialCondition
{
    [SerializeField, Min(0f)] private float threshold = 0.1f;

    public override bool IsMet(TutorialContext context)
    {
        return context.PlayerMovement != null
            && context.PlayerMovement.MoveInput.sqrMagnitude > threshold * threshold;
    }
}

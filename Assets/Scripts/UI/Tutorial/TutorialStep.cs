using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;

[Serializable]
public class TutorialStep
{
    public LocalizedString message = new LocalizedString();
    public bool freezeTime;
    [Min(0)] public float delayBefore;
    [SerializeReference] public TutorialCondition condition;
    public UnityEvent onCompleted;

    public bool HasMessage => message != null && !message.IsEmpty;
}

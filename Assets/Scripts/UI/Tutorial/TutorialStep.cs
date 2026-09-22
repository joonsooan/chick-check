using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class TutorialStep
{
    [TextArea(2, 4)] public string message;
    public bool freezeTime;
    [Min(0)] public float delayBefore;
    [SerializeReference] public TutorialCondition condition;
    public UnityEvent onCompleted;

    public bool HasMessage => !string.IsNullOrEmpty(message);
}

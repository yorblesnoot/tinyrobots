using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class TriggerCondition 
{
    protected TinyBot Target;
    public UnityEvent<TinyBot> OnTriggered = new();
    public TriggerCondition(TinyBot target)
    {
        Target = target;
    }

    protected void ExecuteTrigger(TinyBot target)
    {
        OnTriggered.Invoke(target);   
    }

    protected void ExecuteTrigger()
    {
        ExecuteTrigger(Target);
    }

    public abstract void Remove();
}

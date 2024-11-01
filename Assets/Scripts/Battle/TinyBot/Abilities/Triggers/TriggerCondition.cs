using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class TriggerCondition 
{
    protected TinyBot Target;
    public UnityEvent<TinyBot> OnTriggered = new();
    readonly int procLimit;
    int procCount;
    public TriggerCondition(TinyBot target, int limit)
    {
        Target = target;
        procLimit = limit;
    }

    public void ResetProcCount()
    {
        procCount = 0;
    }

    protected void TryExecuteTrigger(TinyBot target)
    {
        if (procLimit > 0 && procCount >= procLimit) return;
        OnTriggered.Invoke(target);
        procCount++;
    }

    public abstract void Remove();
}

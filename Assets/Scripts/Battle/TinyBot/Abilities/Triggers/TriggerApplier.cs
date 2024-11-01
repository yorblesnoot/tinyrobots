using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class TriggerApplier
{
    enum Condition
    {
        ENTERED,
        DIED,
        HIT,
        ROUNDSTART,
        TURNSTART
    }
    [SerializeField] Condition activationCondition;
    [SerializeField] protected AbilityEffect OutputEffect;
    [SerializeField] int procLimit = 0;
    [SerializeField] Condition procResetCondition;
    protected TinyBot Owner;
    Dictionary<TinyBot, List<TriggerCondition>> activeTriggers;

    public void Initialize(TinyBot owner)
    {
        Owner = owner;
        activeTriggers = new();
    }

    
    public virtual void ApplyTo(TinyBot target)
    {
        if (activeTriggers.ContainsKey(target)) return;
        if (activationCondition == Condition.ENTERED)
        {
            ActivateEffect(target);
            return;
        }
        List<TriggerCondition> triggers = new()
        {
            GetTrigger(target, activationCondition, procLimit, ActivateEffect),
            GetTrigger(target, procResetCondition, 0, ResetLinked)
        };
        activeTriggers.Add(target, triggers);
    }

    private TriggerCondition GetTrigger(TinyBot target, Condition condition, int limit, UnityAction<TinyBot> call)
    {
        TriggerCondition trigger = SetCondition(condition, target, limit);
        trigger.OnTriggered.AddListener(call);
        return trigger;
    }

    void ResetLinked(TinyBot target)
    {
        activeTriggers[target][0].ResetProcCount();
    }

    TriggerCondition SetCondition(Condition condition, TinyBot target, int limit)
    {
        return condition switch
        {
            Condition.DIED => new ConditionDied(target, limit),
            Condition.HIT => throw new System.NotImplementedException(),
            Condition.ROUNDSTART => throw new System.NotImplementedException(),
            Condition.TURNSTART => throw new System.NotImplementedException(),
            Condition.ENTERED => null,
            _ => null
        } ;
    }

    void ActivateEffect(TinyBot target)
    {
        Owner.StartCoroutine(OutputEffect.PerformEffect(Owner, null, new() { target }));
    }

    public virtual void RemoveFrom(TinyBot target)
    {
        List<TriggerCondition> triggers = activeTriggers[target];
        foreach (TriggerCondition triggerCondition in triggers)
        {
            triggerCondition.OnTriggered.RemoveAllListeners();
            triggerCondition.Remove();
        }
        activeTriggers.Remove(target);
    }
}

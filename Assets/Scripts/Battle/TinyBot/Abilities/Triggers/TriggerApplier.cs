using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class TriggerApplier
{
    enum Condition
    {
        ENTERED,
        EXIT,
        DIED,
        HIT,
        ROUNDEND,
        TURNSTART
    }
    [SerializeField] Condition activationCondition;
    [SerializeField] protected AbilityEffect OutputEffect;
    [SerializeField] bool alwaysTargetSelf;
    [SerializeField] int applicationLimit = 0;
    [SerializeField] Condition procResetCondition;
    protected TinyBot Owner;

    Dictionary<TinyBot, TriggerCondition> activeTriggers;
    Dictionary<TinyBot, int> activationCounts = new();

    public void Initialize(TinyBot owner, Ability ability)
    {
        Owner = owner;
        activeTriggers = new();
        OutputEffect.Initialize(ability);
    }

    
    public virtual void ApplyTo(TinyBot target)
    {
        if (activeTriggers.ContainsKey(target)) return;
        if (activationCondition == Condition.ENTERED) ActivateEffect(target);
        else if (activationCondition != Condition.EXIT) activeTriggers.Add(target, GetTrigger(target, activationCondition, ActivateEffect));
    }

    private TriggerCondition GetTrigger(TinyBot target, Condition condition, UnityAction<TinyBot> call)
    {
        TriggerCondition trigger = SetCondition(condition, target);
        trigger.OnTriggered.AddListener(call);
        return trigger;
    }

    void ResetLinked(TinyBot target)
    {
        activationCounts[target] = 0;
        Debug.Log("reset linked");
    }

    TriggerCondition SetCondition(Condition condition, TinyBot target)
    {
        return condition switch
        {
            Condition.DIED => new ConditionDied(target),
            Condition.HIT => throw new System.NotImplementedException(),
            Condition.ROUNDEND => new ConditionRoundStart(target),
            Condition.TURNSTART => throw new System.NotImplementedException(),
            _ => null
        } ;
    }

    void ActivateEffect(TinyBot target)
    {
        if (!activationCounts.ContainsKey(target))
        {
            activationCounts.Add(target, 0);
            GetTrigger(target, procResetCondition, ResetLinked);
        }
        if (applicationLimit > 0 && activationCounts[target] > applicationLimit) return;
        activationCounts[target]++;
        Owner.StartCoroutine(OutputEffect.PerformEffect(Owner, null, new() { alwaysTargetSelf ? Owner : target }));
    }

    public virtual void RemoveFrom(TinyBot target)
    {
        if (activationCondition == Condition.EXIT) ActivateEffect(target);
        else if (activeTriggers.TryGetValue(target, out TriggerCondition trigger))
        {
            trigger.OnTriggered.RemoveAllListeners();
            trigger.Remove();
            activeTriggers.Remove(target);
        }
        
    }
}

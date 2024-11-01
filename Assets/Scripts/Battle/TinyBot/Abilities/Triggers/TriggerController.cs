using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class TriggerController
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
    [SerializeField] protected List<AbilityEffect> OutputEffect;
    [SerializeField] bool alwaysTargetSelf;
    [SerializeField] int activationLimit = 0;
    [SerializeField] Condition procResetCondition;
    protected TinyBot Owner;

    Dictionary<TinyBot, TriggerCondition> activeTriggers;
    Dictionary<TinyBot, int> activationCounts = new();

    public void Initialize(TinyBot owner, Ability ability)
    {
        Owner = owner;
        activeTriggers = new();
        foreach(var effect in OutputEffect) effect.Initialize(ability);
    }

    
    public virtual void ApplyTo(TinyBot target)
    {
        if (activeTriggers.ContainsKey(target)) return;
        if (activationCondition == Condition.ENTERED) ActivateEffect(target);
        else if (activationCondition != Condition.EXIT) activeTriggers.Add(target, GetTrigger(target, activationCondition, ActivateEffect));
    }

    private TriggerCondition GetTrigger(TinyBot target, Condition condition, UnityAction<TinyBot> call)
    {
        TriggerCondition trigger = GetCondition(condition, target);
        trigger.OnTriggered.AddListener(call);
        return trigger;
    }

    void ResetLinked(TinyBot target)
    {
        activationCounts[target] = 0;
    }

    TriggerCondition GetCondition(Condition condition, TinyBot target)
    {
        return condition switch
        {
            Condition.DIED => new ConditionDied(target),
            Condition.HIT => new ConditionHit(target),
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
            if (activationLimit > 0) GetTrigger(target, procResetCondition, ResetLinked);
        }
        if (activationLimit > 0 && activationCounts[target] > activationLimit) return;
        activationCounts[target]++;
        foreach (var effect in OutputEffect) 
            Owner.StartCoroutine(effect.PerformEffect(Owner, null, new() { alwaysTargetSelf ? Owner : target }));
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

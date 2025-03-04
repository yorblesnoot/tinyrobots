using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public abstract class TriggerController 
{
    protected enum Condition
    {
        ENTERED,
        EXIT,
        DIED,
        HIT,
        ROUNDEND,
        TURNSTART
    }
    [SerializeField] Condition activationCondition;
    [SerializeField] protected bool alwaysTargetSelf;
    [SerializeField] int activationLimit = 0;
    protected TinyBot Owner;

    Dictionary<TinyBot, TriggerCondition> activeTriggers;
    Dictionary<TinyBot, int> activationCounts = new();

    public virtual void Initialize(TinyBot owner, Ability ability)
    {
        Owner = owner;
        TinyBot.BotDied.AddListener(CheckForOwnerDeath);
        activeTriggers = new();
    }

    void CheckForOwnerDeath(TinyBot bot)
    {
        if (bot != Owner) return;
        RemoveFrom(Owner);
        TinyBot.BotDied.RemoveListener(CheckForOwnerDeath);
    }


    public virtual void ApplyTo(TinyBot target)
    {
        if (activeTriggers.ContainsKey(target)) return;
        if (activationCondition == Condition.ENTERED) ActivateTrigger(target);
        else if (activationCondition != Condition.EXIT) activeTriggers.Add(target, SetTrigger(target, activationCondition, ActivateTrigger));
    }

    private TriggerCondition SetTrigger(TinyBot target, Condition condition, UnityAction<TinyBot> call)
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
        };
    }

    void ActivateTrigger(TinyBot target)
    {
        if (!activationCounts.ContainsKey(target))
        {
            activationCounts.Add(target, 0);
            if (activationLimit > 0) SetTrigger(target, Condition.ROUNDEND, ResetLinked);
        }
        if (activationLimit > 0 && activationCounts[target] > activationLimit) return;
        activationCounts[target]++;
        ActivateEffect(target);
    }

    protected abstract void ActivateEffect(TinyBot target);

    public virtual void RemoveFrom(TinyBot target)
    {
        if (activationCondition == Condition.EXIT) ActivateTrigger(target);
        else if (activeTriggers.TryGetValue(target, out TriggerCondition trigger))
        {
            trigger.OnTriggered.RemoveAllListeners();
            trigger.Remove();
            activeTriggers.Remove(target);
        }

    }
}

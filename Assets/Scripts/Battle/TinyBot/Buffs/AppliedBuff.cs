using System;
using System.Collections.Generic;
using UnityEngine;

public class AppliedBuff
{
    public BuffType Buff;
    public int Potency;
    public TinyBot Source;
    public TinyBot Target;
    public int Stacks;
    int maxDuration;
    int elapsedDuration = 0;
    BuffEffectCarrier spawnedFX;


    public AppliedBuff(BuffType buff, TinyBot target, TinyBot source, int potency)
    {
        Buff = buff;
        Potency = potency;
        Source = source;
        maxDuration = buff.Duration;
        Target = target;
        elapsedDuration = 0;
    }

    public void ApplyStack()
    {
        elapsedDuration = 0;
        Buff.ApplyEffect(Target, Source, Potency);
        if (Buff.Triggers != null) foreach (BuffTrigger trigger in Buff.Triggers) trigger.ApplyTo(Target);
        if(Stacks == 0 && Buff.FX != null)
        {
            spawnedFX = GameObject.Instantiate(Buff.FX).GetComponent<BuffEffectCarrier>();
            spawnedFX.transform.SetParent(Target.TargetPoint, false);
            spawnedFX.Toggle(true, Source, Target);
        }
        Stacks++;
    }

    public void Remove()
    {
        for(int i = 0; i < Stacks; i++) Buff.RemoveEffect(Target, Potency);
        if(Buff.Triggers != null) foreach (BuffTrigger trigger in Buff.Triggers) trigger.RemoveFrom(Target);
        if(spawnedFX != null) spawnedFX.Toggle(false, Source, Target);
    }

    public bool Tick()
    {
        if (maxDuration == 0) return false;
        elapsedDuration++;
        Buff.TickEffect();
        if (elapsedDuration > maxDuration)
        {
            Remove();
            return true;
        }
        return false;
    }
}

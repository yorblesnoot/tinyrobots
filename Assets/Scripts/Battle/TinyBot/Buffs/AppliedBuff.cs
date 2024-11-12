using System;
using System.Collections.Generic;

public class AppliedBuff
{
    public BuffType Buff;
    public int Potency;
    public TinyBot Source;
    public TinyBot Target;
    public int Stacks;
    int maxDuration;
    int elapsedDuration = 0;


    public AppliedBuff(BuffType buff, TinyBot target, TinyBot source, int potency)
    {
        Buff = buff;
        Potency = potency;
        Source = source;
        //maxDuration = duration;
        Target = target;
        elapsedDuration = 0;
    }

    public void ApplyStack()
    {
        elapsedDuration = 0;
        Buff.ApplyEffect(Target, Source, Potency);
        if (Buff.Triggers != null) foreach (BuffTrigger trigger in Buff.Triggers) trigger.ApplyTo(Target);
        Stacks++;
    }

    public void Remove()
    {
        for(int i = 0; i < Stacks; i++) Buff.RemoveEffect(Target, Potency);
        if(Buff.Triggers != null) foreach (BuffTrigger trigger in Buff.Triggers) trigger.RemoveFrom(Target);
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

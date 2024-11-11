using System;
using System.Collections.Generic;

public class AppliedBuff
{
    public BuffType Buff;
    public int Potency;
    public TinyBot Source;
    public TinyBot Target;
    public int Stacks { get { return stackData.Count; } }
    int maxDuration;
    int elapsedDuration = 0;
    List<object> stackData;


    public AppliedBuff(BuffType buff, TinyBot target, TinyBot source, int potency)
    {
        Buff = buff;
        Potency = potency;
        Source = source;
        //maxDuration = duration;
        Target = target;
        elapsedDuration = 0;
        stackData = new();
    }

    public void ApplyStack()
    {
        elapsedDuration = 0;
        object stack = Buff.ApplyEffect(Target, Source, Potency);
        stackData.Add(stack);
    }

    public void Remove()
    {
        foreach(var stack in stackData) Buff.RemoveEffect(Target, Potency, stack);
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

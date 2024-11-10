using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppliedBuff 
{
    public BuffType Buff;
    public int Potency;
    public TinyBot Source;
    int MaxDuration;
    int ElapsedDuration = 0;


    public AppliedBuff(BuffType buff, int potency, TinyBot source, int duration)
    {
        Buff = buff;
        Potency = potency;
        Source = source;
        MaxDuration = duration;
    }
    
    public void Apply(TinyBot target)
    {
        Buff.ApplyEffect(target, Source, Potency);
    }

    public void Remove(TinyBot target)
    {
        Buff.RemoveEffect(target, Potency);
    }

    public bool CheckBuffIsExpired()
    {
        if (MaxDuration == 0) return false;
        ElapsedDuration++;
        if(ElapsedDuration > MaxDuration) return true;
        return false;
    }
}

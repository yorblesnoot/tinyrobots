using UnityEngine;

public class AppliedBuff
{
    public BuffType Buff;
    public int Potency;
    public TinyBot Source;
    public TinyBot Target;
    int maxDuration;
    int elapsedDuration = 0;
    public int RemainingDuration => maxDuration - elapsedDuration;
    SpawnedEffectCarrier spawnedFX;


    public AppliedBuff(BuffType buff, TinyBot target, TinyBot source, int potency)
    {
        Buff = buff;
        Potency = potency;
        Source = source;
        maxDuration = buff.Duration;
        Target = target;
        elapsedDuration = 0;
    }

    public void Apply()
    {
        elapsedDuration = 0;
        Buff.ApplyEffect(Target, Source, Potency);
        if (Buff.Triggers != null) foreach (BuffTrigger trigger in Buff.Triggers) trigger.ApplyTo(Target);
        if(Buff.FX != null)
        {
            spawnedFX = GameObject.Instantiate(Buff.FX).GetComponent<SpawnedEffectCarrier>();
            spawnedFX.transform.SetParent(Target.TargetPoint, false);
            spawnedFX.Toggle(true, Source, Target);
        }
    }

    public void Remove()
    {
        Buff.RemoveEffect(Target, Potency);
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

    public override string ToString()
    {
        return Buff.name;
    }
}

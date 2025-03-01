public class AppliedDamageFactor 
{
    //this class wraps a damage factor SO and stores data relevant to a specific instance attached to a unit
    public DamageFactor Factor;
    public int Uses;
    public TinyBot Owner;
    int potency;
    public AppliedDamageFactor(DamageFactor factor, int potency, TinyBot owner)
    {
        this.Factor = factor;
        this.potency = potency;
        Owner = owner;
    }

    public float UseFactor(float incomingDamage, TinyBot damageSource, TinyBot damageTarget, bool consume)
    {
        if (Factor.Exclusive && damageSource != Owner) return incomingDamage;
        float output = Factor.UseFactor(incomingDamage, damageSource, damageTarget, potency, Owner);
        if(consume && output != incomingDamage) Uses++;
        return output;
    }
}

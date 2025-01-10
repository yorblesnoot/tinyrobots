public abstract class DamageFactor : BuffType
{
    public int Priority;
    public bool Outgoing = false;
    public int UseLimit;
    public bool Exclusive = false;

    public abstract float UseFactor(float incoming, TinyBot damageSource, TinyBot damageTarget, int potency, TinyBot factorOwner = null);

    public override void ApplyEffect(TinyBot target, TinyBot source, int potency)
    {
        AppliedDamageFactor applied = new(this, potency, source);
        target.DamageCalculator.AddFactor(applied);
    }

    public override void RemoveEffect(TinyBot target, int potency)
    {
        target.DamageCalculator.RemoveFactor(this);
    }
}

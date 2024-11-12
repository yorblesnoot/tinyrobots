public abstract class DamageFactor : BuffType
{
    public int Priority;
    public bool Outgoing = false;

    public abstract float UseFactor(float incoming, TinyBot source, TinyBot target, int potency, object data = null);

    public override void ApplyEffect(TinyBot target, TinyBot source, int potency)
    {
        AppliedDamageFactor applied = GetCustomFactor(target, source, potency);
        target.DamageCalculator.AddFactor(applied);
    }

    public virtual AppliedDamageFactor GetCustomFactor(TinyBot target, TinyBot source, int potency)
    {
        return new(this, potency);
    }

    public override void RemoveEffect(TinyBot target, int potency)
    {
        target.DamageCalculator.RemoveFactor(this);
    }
}

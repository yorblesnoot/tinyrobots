using UnityEngine;
[CreateAssetMenu(fileName = "MultiplierDamage", menuName = "ScriptableObjects/DamageFactors/Multiplier")]
public class MultiplierDamage : DamageFactor
{
    bool universal = true;
    public override AppliedDamageFactor GetCustomFactor(TinyBot target, TinyBot source, int potency)
    {
        return new(this, potency, source);
    }
    public override float UseFactor(float incoming, TinyBot damageSource, TinyBot damageTarget, int potency, object data = null)
    {
        TinyBot factorOwner = data as TinyBot;
        if (!universal && damageSource != factorOwner) return incoming;
        float output = incoming * potency / 100;
        return output;
    }
}

using UnityEngine;
[CreateAssetMenu(fileName = "MultiplierDamage", menuName = "ScriptableObjects/DamageFactors/Multiplier")]
public class MultiplierDamage : DamageFactor
{
    public override string LineDescription => $"% Damage{(Exclusive ? " from User" : "")}";
    public override float UseFactor(float incoming, TinyBot damageSource, TinyBot damageTarget, int potency, TinyBot factorOwner = null)
    {
        float output = incoming * potency / 100;
        return output;
    }
}

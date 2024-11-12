using UnityEngine;
[CreateAssetMenu(fileName = "MultiplierDamage", menuName = "ScriptableObjects/DamageFactors/Multiplier")]
public class MultiplierDamage : DamageFactor
{

    public override float UseFactor(float incoming, TinyBot source, TinyBot target, int potency, object data = null)
    {
        return incoming * potency/100;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BodyguardBuff", menuName = "ScriptableObjects/Buffs/Bodyguard")]
public class BodyguardBuff : BuffType
{
    public override object ApplyEffect(TinyBot target, TinyBot source, int potency)
    {
        float finalPotency = (float)potency / 100;
        RedirectDamage redirect = new(source, finalPotency);
        target.DamageCalculator.AddFactor(redirect);
        return redirect;
    }

    public override void RemoveEffect(TinyBot target, int potency, object data)
    {
        target.DamageCalculator.RemoveFactor(data as RedirectDamage);
    }
}

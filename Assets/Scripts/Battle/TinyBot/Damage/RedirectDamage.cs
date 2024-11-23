using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "RedirectDamage", menuName = "ScriptableObjects/DamageFactors/Redirect")]
public class RedirectDamage : DamageFactor
{
    public override string LineDescription => "% Damage Redirected";
    public override float UseFactor(float incoming, TinyBot source, TinyBot target, int potency, object data = null)
    {
        TinyBot redirectionTarget = data as TinyBot;
        float redirectedDamage = incoming * potency / 100;
        redirectionTarget.ReceiveHit(Mathf.RoundToInt(redirectedDamage), source, redirectionTarget.TargetPoint.position);
        return incoming - redirectedDamage;
    }

    public override AppliedDamageFactor GetCustomFactor(TinyBot target, TinyBot source, int potency)
    {
        return new(this, potency, source);
    }
}

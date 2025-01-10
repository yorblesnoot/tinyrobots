using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "RedirectDamage", menuName = "ScriptableObjects/DamageFactors/Redirect")]
public class RedirectDamage : DamageFactor
{
    public override string LineDescription => "% Damage Redirected";
    public override float UseFactor(float incoming, TinyBot source, TinyBot target, int potency, TinyBot factorOwner = null)
    {
        float redirectedDamage = incoming * potency / 100;
        factorOwner.ReceiveHit(Mathf.FloorToInt(redirectedDamage), source, factorOwner.TargetPoint.position);
        return incoming - redirectedDamage;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BackstabDamage", menuName = "ScriptableObjects/DamageFactors/Backstab")]
public class BackstabDamage : DamageFactor
{
    [SerializeField] float backstabMultiplier = 1.5f;

    public override float UseFactor(float incoming, TinyBot source, TinyBot target, int potency, TinyBot factorOwner = null)
    {
        if (source == null) return incoming;
        Vector3 hitDirection = (source.TargetPoint.position - target.TargetPoint.position).normalized;
        float dot = Vector3.Dot(hitDirection, target.transform.forward);
        bool backstabbed = dot < 0;
        return backstabbed ? backstabMultiplier * incoming : incoming;
    }
}

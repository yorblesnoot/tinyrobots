using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackstabDamage : DamageFactor
{
    readonly float backstabMultiplier = 1.5f;

    public override int Priority => 0;

    public override float UseFactor(float incoming, TinyBot source, TinyBot target)
    {
        if (source == null) return incoming;
        Vector3 hitDirection = (source.TargetPoint.position - target.TargetPoint.position).normalized;
        float dot = Vector3.Dot(hitDirection, target.transform.forward);
        bool backstabbed = dot < 0;
        return backstabbed ? backstabMultiplier * incoming : incoming;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealDamage : AbilityEffect
{
    public override IEnumerator PerformEffect(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        foreach (Targetable target in targets)
        {
            target.ReceiveHit(Ability.EffectMagnitude, owner, target.TargetPoint.position);
        }
        yield break;
    }
}

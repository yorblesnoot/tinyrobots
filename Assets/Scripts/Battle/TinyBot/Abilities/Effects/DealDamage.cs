using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealDamage : AbilityEffect
{
    public override string Description => " Damage";
    [SerializeField] bool damageFlinch = true;
    public override IEnumerator PerformEffect(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        foreach (Targetable target in targets)
        {
            Debug.Log(target.name + " received damage: " + FinalEffectiveness);
            target.ReceiveHit(FinalEffectiveness, owner, target.TargetPoint.position, damageFlinch);
        }
        yield break;
    }
}

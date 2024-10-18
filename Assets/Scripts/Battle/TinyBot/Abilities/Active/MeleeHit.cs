using System.Collections;

public class MeleeHit : ActiveAbility
{
    protected override IEnumerator PerformEffects()
    {
        foreach (Targetable target in CurrentTargets)
        {
            target.ReceiveHit(EffectMagnitude, Owner.transform.position, target.TargetPoint.position);
        }
        yield break;
    }
}

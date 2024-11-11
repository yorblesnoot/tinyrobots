using UnityEngine;

public class RedirectDamage : DamageFactor
{
    public override int Priority => 5;
    readonly TinyBot redirectionTarget;
    readonly float redirectionFactor;

    public RedirectDamage(TinyBot target, float multiplier)
    {
        redirectionTarget = target;
        redirectionFactor = multiplier;
    }

    public override float UseFactor(float incoming, TinyBot source, TinyBot target)
    {
        float redirectedDamage = incoming * redirectionFactor;
        redirectionTarget.ReceiveHit(Mathf.RoundToInt(redirectedDamage), source, redirectionTarget.TargetPoint.position);
        return incoming - redirectedDamage;
    }
}

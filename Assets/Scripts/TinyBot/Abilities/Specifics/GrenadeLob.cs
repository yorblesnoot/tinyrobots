using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeLob : ParabolicAbility
{
    protected override IEnumerator PerformEffects()
    {
        throw new System.NotImplementedException();
    }

    public override List<TinyBot> AimAt(GameObject target, Vector3 sourcePosition, bool aiMode = false)
    {
        List<TinyBot> botTargets = base.AimAt(target, sourcePosition, aiMode);
        return botTargets;
    }
}

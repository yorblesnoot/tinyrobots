using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArmGrab : ActiveAbility
{
    [SerializeField] ArmThrow armThrow;

    protected override IEnumerator PerformEffects()
    {
        Targetable target = currentTargets[0];
        target.ToggleActiveLayer(true);
        Pathfinder3D.EvaluateNodeOccupancy(Owner.transform.position);
        target.transform.SetParent(emissionPoint.transform, true);
        armThrow.PrepareToThrow(target);
        yield return null;
    }

    public override bool IsUsable(Vector3 targetPosition)
    {
        if(currentTargets.Count == 0) return false;
        return true;
    }
}

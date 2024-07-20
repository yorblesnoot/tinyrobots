using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArmGrab : ActiveAbility
{
    [SerializeField] ArmThrow armThrow;
    [HideInInspector] public Targetable grabbed;
    protected override IEnumerator PerformEffects()
    {
        Targetable target = currentTargets[0];
        target.ToggleActiveLayer(true);
        Pathfinder3D.EvaluateNodeOccupancy(Owner.transform.position);
        target.transform.SetParent(emissionPoint.transform, true);
        armThrow.PrepareToThrow();
        yield return null;
    }

    public override bool IsUsable(Vector3 targetPosition)
    {
        if(currentTargets.Count == 0) return false;
        return true;
    }

    public override void EndAbility()
    {
        base.EndAbility();
        if (grabbed == null) return;
        EndGrab();
        StartCoroutine(grabbed.Fall());
    }

    public void EndGrab()
    {
        grabbed.ToggleActiveLayer(false);
        grabbed.transform.SetParent(null, true);
        Owner.EndedTurn.RemoveListener(EndAbility);
    }
}

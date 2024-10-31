using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapUnits : AbilityEffect
{
    public override IEnumerator PerformEffect(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        Pathfinder3D.GetLandingPointBy(targets[0].transform.position, owner.MoveStyle, out Vector3Int cleanTarget);
        Pathfinder3D.GetLandingPointBy(owner.transform.position, targets[0].MoveStyle, out Vector3Int cleanOrigin);
        owner.transform.position = cleanTarget;
        targets[0].transform.position = cleanOrigin;
        StartCoroutine(owner.Fall());
        StartCoroutine(targets[0].Fall());
        yield break;
    }
}

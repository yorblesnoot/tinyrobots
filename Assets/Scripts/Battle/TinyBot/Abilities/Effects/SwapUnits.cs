using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapUnits : AbilityEffect
{
    public override IEnumerator PerformEffect(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        owner.transform.GetPositionAndRotation(out Vector3 originPosition, out Quaternion originRotation);
        owner.transform.SetPositionAndRotation(targets[0].transform.position, targets[0].transform.rotation);
        targets[0].transform.SetPositionAndRotation(originPosition, originRotation);
        StartCoroutine(owner.Fall());
        StartCoroutine(targets[0].Fall());
        yield break;
    }
}

using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabTracker : TrackingAnimation
{
    [SerializeField] float carryHeight = 1;
    [SerializeField] float armMoveDuration = .5f;
    public override void Aim(List<Vector3> trajectory)
    {
        Vector3 targetPosition = Vector3.Lerp(ikTarget.position, endPoint, Time.deltaTime);
        ikTarget.transform.position = targetPosition;
    }

    public override IEnumerator PreAbility(List<Vector3> trajectory, List<Targetable> targets)
    {
        yield return Tween.Position(ikTarget, endValue: targets[0].TargetPoint.position, duration: armMoveDuration).ToYieldInstruction();
    }    
}

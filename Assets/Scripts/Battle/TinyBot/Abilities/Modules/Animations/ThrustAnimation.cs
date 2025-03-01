using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrustAnimation : IKAnimation
{
    [SerializeField] float thrustLength = 3;
    [SerializeField] float thrustDuration = .3f;
    [SerializeField] float thrustLinger = .2f;
    [SerializeField] float returnDuration = .5f;
    public override IEnumerator PerformEffect(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        Vector3 point = trajectory[^1];
        Vector3 direction = point - trajectory[0];
        direction.Normalize();
        Vector3 thrustTarget = ikTarget.transform.position + direction * thrustLength;
        //StartCoroutine(owner.Movement.ApplyImpulseToBody(direction, 1, thrustDuration, returnDuration));
        yield return Tween.Position(ikTarget, thrustTarget, thrustDuration, ease: Ease.InCubic).ToYieldInstruction();
        yield return new WaitForSeconds(thrustLinger);
    }
}

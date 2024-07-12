using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabTargetAnimation : IKAnimation
{
    [SerializeField] float armMoveDuration = .5f;
    public override IEnumerator Play(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        yield return Tween.Position(ikTarget, endValue: targets[0].TargetPoint.position, duration: armMoveDuration).ToYieldInstruction();
    }
}

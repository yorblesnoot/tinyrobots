using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowWindupAnimation : IKAnimation
{
    [SerializeField] float windupTime = 1f;
    [SerializeField] float windDistance = 1f;
    public override IEnumerator PerformEffect(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        Vector3 windDirection = trajectory[0] - trajectory[1];
        windDirection.Normalize();
        Vector3 windTarget = (windDirection * windDistance) + ikTarget.transform.position;
        yield return Tween.Position(ikTarget, endValue: windTarget, duration: windupTime)
            .Chain(Tween.Position(ikTarget, endValue: trajectory[0], duration: windupTime / 2)).ToYieldInstruction();
    }
}

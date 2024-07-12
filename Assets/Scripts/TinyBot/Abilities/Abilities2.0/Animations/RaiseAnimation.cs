using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaiseAnimation : ProceduralAnimation
{
    [SerializeField] float raiseHeight = 1.0f;
    [SerializeField] float armMoveDuration = .5f;
    public override IEnumerator Play(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        Vector3 holdPosition = owner.transform.up * raiseHeight;
        holdPosition += transform.position + owner.transform.forward;
        yield return Tween.Position(ikTarget, endValue: holdPosition, duration: armMoveDuration).ToYieldInstruction();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpulseAnimation : AbilityEffect
{
    [SerializeField] float impulseLength = .5f;
    [SerializeField] float duration = .05f;
    [SerializeField] float returnDuration = .1f;
    [SerializeField] bool waitForEnd = false;
    public override IEnumerator PerformEffect(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        Vector3 direction = trajectory[0] - trajectory[^1];
        if(direction == Vector3.zero) direction = owner.transform.forward;
        if(waitForEnd) yield return StartCoroutine(owner.Movement.ApplyImpulseToBody(direction, impulseLength, duration, returnDuration));
        else StartCoroutine(owner.Movement.ApplyImpulseToBody(direction, impulseLength, duration, returnDuration));
    }
}

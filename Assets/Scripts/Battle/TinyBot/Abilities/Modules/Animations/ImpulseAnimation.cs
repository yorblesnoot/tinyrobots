using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpulseAnimation : AbilityEffect
{
    [SerializeField] bool upDown = false;
    [SerializeField] float impulseLength = .5f;
    [SerializeField] float duration = .05f;
    [SerializeField] float returnDuration = .1f;
    [SerializeField] float waitDuration = .5f;
    public override IEnumerator PerformEffect(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        Vector3 direction = trajectory[0] - trajectory[^1];
        if (upDown) direction = owner.transform.up;
        else if(direction == Vector3.zero) direction = owner.transform.forward;
        StartCoroutine(owner.Movement.ApplyImpulseToBody(direction, impulseLength, duration, returnDuration));
        yield return new WaitForSeconds(waitDuration);
    }
}

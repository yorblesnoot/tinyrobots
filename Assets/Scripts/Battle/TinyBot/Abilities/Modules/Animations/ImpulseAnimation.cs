using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpulseAnimation : AnimationController
{
    [SerializeField] float impulseLength = .5f;
    [SerializeField] float duration = .05f;
    [SerializeField] float returnDuration = .1f;
    public override IEnumerator Play(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        Vector3 direction = trajectory[0] - trajectory[^1];
        StartCoroutine(owner.PrimaryMovement.ApplyImpulseToBody(direction, impulseLength, duration, returnDuration));
        yield break;
    }
}

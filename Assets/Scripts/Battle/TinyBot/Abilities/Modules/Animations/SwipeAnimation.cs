using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeAnimation : IKAnimation
{
    [SerializeField] float swingTime = .3f;
    [SerializeField][Range(0,1)] float swipeWidth = .5f;
    public override IEnumerator PerformEffect(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        Vector3 swingSource = Ability.transform.position;
        Vector3 targetDirection = trajectory[^1] - swingSource;
        Debug.DrawLine(swingSource, swingSource + targetDirection, Color.green, 10f);

        Vector3 swingDirection = owner.TargetPoint.position - trajectory[0];
        Vector3 midSwing = Vector3.Slerp(targetDirection, swingDirection, .5f);
        Debug.DrawLine(swingSource, swingSource + midSwing, Color.magenta, 10f);

        Vector3 reverseSwing = Vector3.Reflect(-midSwing, targetDirection.normalized);
        Debug.Log(reverseSwing.magnitude);
        Debug.DrawLine(swingSource, swingSource + reverseSwing, Color.blue, 10f);

        yield return Tween.Position(ikTarget, reverseSwing + swingSource, duration: swingTime).ToYieldInstruction();
        StartCoroutine(owner.Movement.ApplyImpulseToBody(targetDirection, 1, swingTime, swingTime * 2));
        float timeElapsed = 0;
        while(timeElapsed < swingTime)
        {
            float progress = timeElapsed / swingTime;
            Vector3 point = Vector3.Slerp(reverseSwing, midSwing, progress);
            point += swingSource;
            ikTarget.position = point;
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        swingDirection.Normalize();
    }
}

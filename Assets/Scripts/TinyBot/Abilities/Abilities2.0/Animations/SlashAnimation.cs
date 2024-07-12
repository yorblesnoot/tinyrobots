using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlashAnimation : ProceduralAnimation
{
    [SerializeField] Transform slashPosition;

    [SerializeField] float slashHeight = 1;
    [SerializeField] float slashWidth = 1;
    [SerializeField] float slashLength = 3;

    [SerializeField] float slashTime = .07f;
    [SerializeField] float returnTime = .4f;
    public override IEnumerator Play(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        Vector3[] slashPoints = GetSlashPoints(owner);

        Vector3 thrustTarget = (slashPoints[0] + slashPoints[1]) / 2;
        yield return StartCoroutine(ikTarget.gameObject.LerpTo(slashPoints[0], slashTime));
        StartCoroutine(owner.PrimaryMovement.ApplyImpulseToBody(thrustTarget, -1, slashTime, returnTime * 2));
        yield return StartCoroutine(SlashThroughPoints(owner, slashPoints, slashTime));
    }

    IEnumerator SlashThroughPoints(TinyBot owner, Vector3[] points, float duration)
    {
        float timeElapsed = 0;
        Vector3 startPosition = owner.transform.InverseTransformPoint(points[0]);
        Vector3 endPosition = owner.transform.InverseTransformPoint(points[1]);
        while (timeElapsed < duration)
        {
            Vector3 localPosition = Vector3.Slerp(startPosition, endPosition, timeElapsed / duration);
            ikTarget.transform.position = owner.transform.TransformPoint(localPosition);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
    }

    Vector3[] GetSlashPoints(TinyBot owner)
    {
        Vector3 awayFromBody = (slashPosition.position - owner.TargetPoint.position).normalized;
        Vector3 startPosition = slashPosition.position + owner.transform.up * slashHeight + awayFromBody * slashWidth;

        Vector3 lastPosition = Vector3.Reflect(-owner.TargetPoint.InverseTransformPoint(startPosition), Vector3.forward);
        lastPosition = owner.TargetPoint.TransformPoint(lastPosition);
        return new Vector3[] { startPosition, lastPosition };
    }
}

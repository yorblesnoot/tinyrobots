using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SegmentedMovement : LegMovement
{
    public SegmentedMovement ChildSegment;
    public float SegmentLength = 1f;

    public override IEnumerator TraversePath(List<Vector3> path)
    {
        if(ChildSegment != null)
        {
            StartCoroutine(ChildSegment.TraversePath(GetSubPath(path)));
        }
        
        foreach (var target in path)
        {
            yield return StartCoroutine(InterpolatePositionAndRotation(Owner.transform, target));
        }
    }

    List<Vector3> GetSubPath(List<Vector3> path)
    {
        List<Vector3> subPath = new(path);
        Vector3 offset = subPath[^1] - subPath[^2];
        if(offset.magnitude > SegmentLength)
        {
            offset.Normalize();
            offset *= SegmentLength;
        }
        subPath[^1] = subPath[^2] + offset;
        subPath.Insert(0, Owner.transform.position);

        return subPath;
    }
}



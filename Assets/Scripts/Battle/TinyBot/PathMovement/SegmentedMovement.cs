using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SegmentedMovement : LegMovement
{
    public SegmentedMovement ChildSegment;

    public override IEnumerator TraversePath(List<Vector3> path)
    {
        List<Vector3> subPath = new(path);
        if(ChildSegment != null)
        {
            subPath.RemoveAt(subPath.Count - 1);
            subPath.Insert(0, Owner.transform.position);
            StartCoroutine(ChildSegment.TraversePath(subPath));
        }
        
        foreach (var target in path)
        {
            yield return StartCoroutine(InterpolatePositionAndRotation(Owner.transform, target));
        }
    }
}

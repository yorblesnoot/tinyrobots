using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NoTrajectory : Trajectory
{
    readonly float endSize = .1f;
    Collider[] colliders = new Collider[1];
    public override List<Vector3> GetTrajectory(Vector3 sourcePosition, Vector3 target, out Collider collider)
    {
        Physics.OverlapSphereNonAlloc(target, endSize, colliders, BlockingLayerMask);
        collider = colliders[0];
        return CalculateTrajectory(sourcePosition, target).ToList();
    }
    protected override Vector3[] CalculateTrajectory(Vector3 source, Vector3 target)
    {
        return new Vector3[] { source, target };
    }
}

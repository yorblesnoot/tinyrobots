using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NoTrajectory : Trajectory
{
    public override List<Vector3> GetTrajectory(Vector3 sourcePosition, Vector3 target, out RaycastHit hit, bool aiMode = false)
    {
        hit = default;
        return CalculateTrajectory(sourcePosition, target).ToList();
    }
    protected override Vector3[] CalculateTrajectory(Vector3 source, Vector3 target)
    {
        return new Vector3[] { source, target };
    }
}

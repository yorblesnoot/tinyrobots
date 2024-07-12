using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoTrajectory : Trajectory
{
    protected override Vector3[] CalculateTrajectory(Vector3 source, Vector3 target)
    {
        return new Vector3[] { target };
    }
}

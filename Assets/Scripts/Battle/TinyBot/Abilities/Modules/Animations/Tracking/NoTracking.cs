using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoTracking : TrackingAnimation
{
    public override void Aim(List<Vector3> trajectory)
    {
        return;
    }
}

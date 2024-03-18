using System.Collections.Generic;
using UnityEngine;

public class TurretTracker : TrackingAnimation
{
    [SerializeField] float slowness;

    public override void TrackTrajectory(List<Vector3> trajectory)
    {
        Vector3 localizedTarget = trajectory[^1];
        localizedTarget = transform.InverseTransformPoint(localizedTarget);
        aimTarget.localPosition = Vector3.Lerp(aimTarget.localPosition, localizedTarget, 1/slowness);
    }
}

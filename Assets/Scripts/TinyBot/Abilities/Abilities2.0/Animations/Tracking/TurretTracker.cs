using System.Collections.Generic;
using UnityEngine;

public class TurretTracker : TrackingAnimation
{
    [SerializeField] float slowness;

    public override void Aim(List<Vector3> trajectory)
    {
        Vector3 localizedTarget = trajectory[^1];
        localizedTarget = transform.InverseTransformPoint(localizedTarget);
        ikTarget.localPosition = Vector3.Lerp(ikTarget.localPosition, localizedTarget, 1/slowness);
    }
}

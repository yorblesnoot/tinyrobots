using System.Collections.Generic;
using UnityEngine;

public class TurretTracker : TrackingAnimation
{
    [SerializeField] float slowness;

    public override void Aim(List<Vector3> trajectory)
    {
        ikTarget.position = Vector3.Lerp(ikTarget.position, trajectory[^1], 1/slowness);
    }

}

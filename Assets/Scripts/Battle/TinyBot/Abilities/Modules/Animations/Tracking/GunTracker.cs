using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunTracker : TrackingAnimation
{
    [SerializeField] float aimDistance = 1f;
    [SerializeField] Transform aimPoint; 

    public override void Aim(List<Vector3> trajectory)
    {
        Vector3 direction = trajectory[1] - trajectory[0];
        direction.Normalize();
        ikTarget.position = aimPoint.position + direction * aimDistance;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordTracker : TrackingAnimation
{
    [SerializeField] Transform readyPosition;
    [SerializeField] float aimLag = 30;
    public override void Aim(List<Vector3> trajectory)
    {
        ikTarget.position = Vector3.Lerp(ikTarget.position, readyPosition.position, 1 / aimLag);
    }

    public override void ResetTracking()
    {
        base.ResetTracking();
    }
}

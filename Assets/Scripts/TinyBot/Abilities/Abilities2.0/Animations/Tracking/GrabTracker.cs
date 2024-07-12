using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabTracker : TrackingAnimation
{
    public override void Aim(List<Vector3> trajectory)
    {
        Vector3 targetPosition = Vector3.Lerp(ikTarget.position, trajectory[^1], Time.deltaTime);
        ikTarget.transform.position = targetPosition;
    }   
}

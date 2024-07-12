using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldTracker : TrackingAnimation
{
    [SerializeField] float shieldDistance = .7f;
    public override void Aim(List<Vector3> trajectory)
    {
        Vector3 ownerPosition = transform.position;
        Vector3 direction = trajectory[^1] - ownerPosition;
        direction.Normalize();
        direction *= shieldDistance;
        Vector3 finalPosition = ownerPosition + direction;
        ikTarget.transform.position = finalPosition;
    }
}

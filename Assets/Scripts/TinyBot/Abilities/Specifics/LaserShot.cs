using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserShot : LinearAbility
{
    [SerializeField] GameObject laser;
    [SerializeField] float travelTime;
    [SerializeField] TurretTracker turretTracker;
    [SerializeField] int damage;
    public override IEnumerator ExecuteAbility()
    {
        Vector3[] trajectory = GetTrajectory(emissionPoint.transform.position, targetedPosition);
        List<Vector3> points = CastAlongPoints(trajectory, blockingLayerMask, out var hit);
        yield return StartCoroutine(LaunchAlongLine(laser, points, travelTime, hit));
    }

    protected override void CompleteTrajectory(Vector3 position, GameObject launched, GameObject hit)
    {
        Destroy(launched);
        if (hit != null && hit.TryGetComponent(out TinyBot bot)) bot.ReceiveDamage(damage);
    }

    protected override void AimAt(GameObject target)
    {
        base.AimAt(target);
        turretTracker.TrackTarget(target);
    }
}

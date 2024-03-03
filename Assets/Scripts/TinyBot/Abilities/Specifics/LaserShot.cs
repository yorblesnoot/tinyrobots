using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserShot : LinearAbility
{
    [SerializeField] GameObject laser;
    [SerializeField] float travelTime;
    [SerializeField] TurretTracker turretTracker;
    protected override IEnumerator PerformEffects()
    {
        List<Vector3> points = CastAlongPoints(targetTrajectory.ToArray(), blockingLayerMask, out var hit);
        yield return StartCoroutine(LaunchAlongLine(laser, points, travelTime, hit));
    }

    protected override void CompleteTrajectory(Vector3 position, GameObject launched, RaycastHit hit)
    {
        Destroy(launched);
        if (hit.collider != null && hit.collider.TryGetComponent(out TinyBot bot)) bot.ReceiveDamage(damage, Owner.transform.position, hit.point);
    }

    public override List<TinyBot> GhostAimAt(GameObject target, Vector3 sourcePosition)
    {
        return base.AimAt(target, sourcePosition);
    }

    protected override List<TinyBot> AimAt(GameObject target, Vector3 sourcePosition)
    {
        turretTracker.TrackTarget(target);
        return base.AimAt(target, sourcePosition);
    }
}

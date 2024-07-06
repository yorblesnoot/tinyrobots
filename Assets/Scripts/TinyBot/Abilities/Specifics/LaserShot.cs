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
        yield return StartCoroutine(LaunchAlongLine(laser, travelTime));
        NeutralAim();
    }

    protected override void CompleteTrajectory(Vector3 position, GameObject launched, RaycastHit hit)
    {
        Destroy(launched);
        if (hit.collider != null && hit.collider.TryGetComponent(out TinyBot bot)) bot.ReceiveDamage(damage, Owner.transform.position, hit.point);
    }

    public override List<TinyBot> AimAt(GameObject target, Vector3 sourcePosition, bool aiMode = false)
    {
        List <TinyBot> hits = base.AimAt(target, sourcePosition, aiMode);
        if (!aiMode) turretTracker.TrackTrajectory(targetTrajectory);
        return hits;
    }

    public override void NeutralAim()
    {
        turretTracker.ResetTracking();
    }
}

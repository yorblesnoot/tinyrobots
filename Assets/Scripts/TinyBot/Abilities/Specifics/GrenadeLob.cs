using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeLob : ParabolicAbility
{
    [SerializeField] GameObject grenade;
    [SerializeField] float travelTime;
    [SerializeField] GunTracker gunTracker;
    [SerializeField] ExplosiveModule explosiveModule;
    protected override IEnumerator PerformEffects()
    {
        List<Vector3> points = CastAlongPoints(targetTrajectory.ToArray(), blockingLayerMask, out var hit);
        yield return StartCoroutine(LaunchAlongLine(grenade, points, travelTime, hit));
        gunTracker.ResetTracking();
        explosiveModule.HideIndicator();
    }

    protected override void CompleteTrajectory(Vector3 position, GameObject launched, RaycastHit hit)
    {
        StartCoroutine(explosiveModule.Detonate(position, damage));
        Destroy(launched);
    }

    public override void ReleaseLock()
    {
        base.ReleaseLock();
        explosiveModule.HideIndicator();
    }

    public override List<TinyBot> AimAt(GameObject target, Vector3 sourcePosition, bool aiMode = false)
    {
        List<TinyBot> hits = base.AimAt(target, sourcePosition, aiMode);
        gunTracker.TrackTrajectory(targetTrajectory);
        List<TinyBot> explosiveHits = explosiveModule.CheckExplosionZone(targetTrajectory[^1], aiMode);
        foreach (TinyBot bot in explosiveHits)
        {
            if(hits.Contains(bot)) continue;
            hits.Add(bot);
        }
        return hits;
    }
}

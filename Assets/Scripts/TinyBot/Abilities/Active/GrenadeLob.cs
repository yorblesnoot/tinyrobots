using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeLob : ParabolicTrajectory
{
    [SerializeField] GameObject grenade;
    [SerializeField] float travelTime;
    [SerializeField] GunTracker gunTracker;
    [SerializeField] ExplosiveModule explosiveModule;
    protected override IEnumerator PerformEffects()
    {
        yield return StartCoroutine(LaunchAlongLine(grenade, travelTime));
        NeutralAim();
    }

    protected override void CompleteTrajectory(Vector3 position, GameObject launched, RaycastHit hit)
    {
        StartCoroutine(explosiveModule.Detonate(position, damage));
        Destroy(launched);
    }

    public override void ReleaseLockOn()
    {
        base.ReleaseLockOn();
        explosiveModule.HideIndicator();
    }

    public override List<Targetable> AimAt(GameObject target, Vector3 sourcePosition, bool aiMode = false)
    {
        List<Targetable> hits = base.AimAt(target, sourcePosition, aiMode);
        gunTracker.TrackTrajectory(targetTrajectory);
        List<Targetable> explosiveHits = explosiveModule.CheckExplosionZone(targetTrajectory[^1], aiMode);
        foreach (Targetable hit in explosiveHits)
        {
            if(hits.Contains(hit)) continue;
            hits.Add(hit);
        }
        return hits;
    }

    public override void NeutralAim()
    {
        gunTracker.ResetTracking();
        explosiveModule.HideIndicator();
    }
}

using System.Collections;
using UnityEngine;

public class GrenadeLob : ShootProjectile
{
    [SerializeField] ExplosiveModule explosiveModule; 
    protected override IEnumerator PerformEffects()
    {
        GameObject spawned = Instantiate(projectile);
        yield return StartCoroutine(ProjectileMovement.LaunchAlongLine(spawned, travelTime, CurrentTrajectory, CompleteTrajectory));
    }

    protected override void CompleteTrajectory(Vector3 position, GameObject launched)
    {
        StartCoroutine(explosiveModule.Detonate(CurrentTargets, CurrentTrajectory[^1], EffectMagnitude));
        Destroy(launched);
    }
}

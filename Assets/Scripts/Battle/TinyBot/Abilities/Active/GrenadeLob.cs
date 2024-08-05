using System.Collections;
using UnityEngine;

public class GrenadeLob : ShootProjectile
{
    [SerializeField] ExplosiveModule explosiveModule; 
    protected override IEnumerator PerformEffects()
    {
        GameObject spawned = Instantiate(projectile);
        yield return StartCoroutine(ProjectileMovement.LaunchAlongLine(spawned, travelTime, currentTrajectory, CompleteTrajectory));
    }

    protected override void CompleteTrajectory(Vector3 position, GameObject launched)
    {
        StartCoroutine(explosiveModule.Detonate(currentTargets, currentTrajectory[^1], damage));
        Destroy(launched);
    }
}

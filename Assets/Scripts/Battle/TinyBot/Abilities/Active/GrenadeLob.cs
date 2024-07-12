using System.Collections;
using UnityEngine;

public class GrenadeLob : ProjectileAbility
{
    [SerializeField] ExplosiveModule explosiveModule; 
    protected override IEnumerator PerformEffects()
    {
        GameObject spawned = Instantiate(projectile);
        yield return StartCoroutine(LaunchAlongLine(spawned, travelTime));
        StartCoroutine(explosiveModule.Detonate(currentTargets, currentTrajectory[^1], damage));
        Destroy(spawned);
    }
}

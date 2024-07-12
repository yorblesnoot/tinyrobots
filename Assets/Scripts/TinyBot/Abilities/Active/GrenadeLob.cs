using System.Collections;
using UnityEngine;

public class GrenadeLob : ProjectileShot
{
    [SerializeField] GameObject grenade;
    [SerializeField] ExplosiveModule explosiveModule; 
    protected override IEnumerator PerformEffects()
    {
        GameObject spawned = Instantiate(grenade);
        yield return StartCoroutine(LaunchAlongLine(spawned, travelTime));
        StartCoroutine(explosiveModule.Detonate(currentTargets, currentTrajectory[^1], damage));
        Destroy(spawned);
    }
}

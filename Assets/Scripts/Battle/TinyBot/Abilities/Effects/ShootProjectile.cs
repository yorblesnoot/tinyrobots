using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootProjectile : AbilityEffect
{
    [SerializeField] protected GameObject projectile;
    [SerializeField] protected float travelTime = 1;

    public override IEnumerator PerformEffect(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        GameObject spawned = Instantiate(projectile);
        yield return StartCoroutine(ProjectileMovement.LaunchAlongLine(spawned, travelTime, trajectory));
        Destroy(spawned);
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShootProjectile : ActiveAbility
{
    [SerializeField] protected GameObject projectile;
    [SerializeField] protected float travelTime = 1;
    protected override IEnumerator PerformEffects()
    {
        GameObject spawned = Instantiate(projectile);
        yield return StartCoroutine(ProjectileMovement.LaunchAlongLine(spawned, travelTime, currentTrajectory, CompleteTrajectory));
    }

    protected virtual void CompleteTrajectory(Vector3 position, GameObject launched)
    {
        Destroy(launched);
        foreach (var targetable in currentTargets)
        {
            targetable.ReceiveHit(damage, Owner.transform.position, currentTrajectory[^1]);
        }
    }
}

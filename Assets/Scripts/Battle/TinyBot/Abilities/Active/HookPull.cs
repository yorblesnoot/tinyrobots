using System.Collections;
using UnityEngine;

public class HookPull : HookAbility
{
    [SerializeField] float dropDistance = 1;
    [SerializeField] float pullDelay = .5f;
    
    protected override IEnumerator PerformEffects()
    {
        line.positionCount = 2;
        projectile.transform.SetParent(null, true);

        float intervalTime = travelTime / currentTrajectory.Count;
        Targetable target = null;
        if (currentTargets != null && currentTargets.Count > 0)
        {
            target = currentTargets[0];
            currentTrajectory[^1] = currentTargets[0].TargetPoint.position;
        }
        
        yield return StartCoroutine(LaunchWithLine(projectile, currentTrajectory, intervalTime));
        currentTrajectory.Reverse();
        Vector3 direction = (currentTrajectory[0] - currentTrajectory[^1]).normalized;
        currentTrajectory[^1] = emissionPoint.transform.position + direction * dropDistance;

        if (target != null)
        {
            target.ReceiveHit(damage, Owner.transform.position, currentTrajectory[^1]);
            if (target.IsDead) target = null;
            else
            {
                yield return new WaitForSeconds(pullDelay);
                StartCoroutine(ProjectileMovement.LaunchAlongLine(target.gameObject, travelTime, currentTrajectory));
            }
        }
        yield return StartCoroutine(LaunchWithLine(projectile, currentTrajectory, intervalTime, false));
        EndAbility();
        if (target != null) yield return StartCoroutine(target.Fall());
    }
}

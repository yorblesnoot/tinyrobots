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

        float intervalTime = travelTime / CurrentTrajectory.Count;
        Targetable target = null;
        if (CurrentTargets != null && CurrentTargets.Count > 0)
        {
            target = CurrentTargets[0];
            CurrentTrajectory[^1] = CurrentTargets[0].TargetPoint.position;
        }
        
        yield return StartCoroutine(LaunchWithLine(projectile, CurrentTrajectory, intervalTime));
        CurrentTrajectory.Reverse();
        Vector3 direction = (CurrentTrajectory[0] - CurrentTrajectory[^1]).normalized;
        CurrentTrajectory[^1] = emissionPoint.transform.position + direction * dropDistance;

        if (target != null)
        {
            target.ReceiveHit(damage, Owner.transform.position, CurrentTrajectory[^1]);
            if (target.IsDead) target = null;
            else
            {
                yield return new WaitForSeconds(pullDelay);
                StartCoroutine(ProjectileMovement.LaunchAlongLine(target.gameObject, travelTime, CurrentTrajectory));
            }
        }
        yield return StartCoroutine(LaunchWithLine(projectile, CurrentTrajectory, intervalTime, false));
        EndAbility();
        if (target != null) yield return StartCoroutine(target.Fall());
    }
}

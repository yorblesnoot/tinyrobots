using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookPull : HookAbility
{
    [SerializeField] float dropDistance = 1;
    [SerializeField] float pullDelay = .5f;
    public override string Description => " Damage";

    public override IEnumerator PerformEffect(TinyBot owner, List<Vector3> trajectory, List<Targetable> currentTargets)
    {
        line.positionCount = 2;
        projectile.transform.SetParent(null, true);

        float intervalTime = travelTime / trajectory.Count;
        Targetable target = null;
        if (currentTargets != null && currentTargets.Count > 0)
        {
            target = currentTargets[0];
            trajectory[^1] = currentTargets[0].TargetPoint.position;
        }
        
        yield return StartCoroutine(LaunchWithLine(projectile, trajectory, intervalTime));
        trajectory.Reverse();
        Vector3 direction = (trajectory[0] - trajectory[^1]).normalized;
        trajectory[^1] = Ability.emissionPoint.position + direction * dropDistance;

        if (target != null)
        {
            target.ReceiveHit(FinalEffectiveness, owner, trajectory[^1]);
            if (target.IsDead) target = null;
            else
            {
                yield return new WaitForSeconds(pullDelay);
                StartCoroutine(ProjectileMovement.LaunchAlongLine(target.gameObject, travelTime, trajectory));
            }
        }
        yield return StartCoroutine(LaunchWithLine(projectile, trajectory, intervalTime, false));
        ResetHook();
        if (target != null) yield return StartCoroutine(target.Fall());
        Pathfinder3D.EvaluateNodeOccupancy(owner.transform.position);
    }
}

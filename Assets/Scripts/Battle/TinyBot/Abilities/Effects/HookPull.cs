using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookPull : HookAbility
{
    [SerializeField] float dropDistance = 1;
    [SerializeField] float pullDelay = .5f;
    [SerializeField] float returnVelocity = 5;
    public override string Description => " Damage";

    public override IEnumerator PerformEffect(TinyBot owner, List<Vector3> trajectory, List<Targetable> currentTargets)
    {
        line.positionCount = 2;
        projectile.transform.SetParent(null, true);

        Targetable target = null;
        if (currentTargets != null && currentTargets.Count > 0)
        {
            target = currentTargets[0];
            trajectory[^1] = currentTargets[0].TargetPoint.position;
        }
        
        yield return StartCoroutine(LaunchWithLine(projectile, trajectory, TravelSpeed));
        trajectory.Reverse();
        Vector3 direction = (trajectory[0] - trajectory[^1]).normalized;
        trajectory[^1] = Ability.emissionPoint.position + direction * dropDistance;
        Transform previousParent = null;

        if (target != null)
        {
            target.ReceiveHit(FinalEffectiveness, owner, trajectory[0]);
            if (target.IsDead) target = null;
            else
            {
                yield return new WaitForSeconds(pullDelay);
                previousParent = target.transform.parent;
                target.transform.SetParent(projectile.transform, true);
            }
        }
        yield return StartCoroutine(LaunchWithLine(projectile, trajectory, returnVelocity, false));
        if (previousParent != null)
        {
            target.transform.SetParent(previousParent, true);
        }
        ResetHook();
        if (target != null) yield return StartCoroutine(target.Fall());
        Pathfinder3D.EvaluateNodeOccupancy(owner.transform.position);
    }
}

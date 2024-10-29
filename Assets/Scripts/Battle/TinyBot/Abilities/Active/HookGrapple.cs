using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HookGrapple : HookAbility
{
    [SerializeField] float backDistance = 2;
    
    protected override IEnumerator PerformEffects()
    {
        line.positionCount = 2;
        projectile.transform.SetParent(null, true);
        
        float intervalTime = travelTime / CurrentTrajectory.Count;
        yield return StartCoroutine(LaunchWithLine(projectile, CurrentTrajectory, intervalTime));
        bool hitSomething = Vector3.Distance(CurrentTrajectory[0], CurrentTrajectory[^1]) < range;

        if (CurrentTargets.Count > 0) CurrentTargets[0].ReceiveHit(EffectMagnitude, Owner, CurrentTrajectory[^1]);
        Vector3 backDirection = CurrentTrajectory[1] - CurrentTrajectory[0];
        backDirection.Normalize();
        backDirection *= backDistance;
        Vector3 secondTarget = CurrentTrajectory[1] - backDirection;
        List<Vector3> secondaryTrajectory = new() { CurrentTrajectory[0], secondTarget };
        yield return StartCoroutine(LaunchWithLine(Owner.gameObject, secondaryTrajectory, intervalTime));
        
        EndAbility();
        if (hitSomething)
        {
            yield return StartCoroutine(Owner.Fall());
            Pathfinder3D.GeneratePathingTree(Owner.MoveStyle, Owner.transform.position);
        }
    }

    public override bool IsUsable(Vector3 targetPosition)
    {
        return TrajectoryCollided;
    }

}

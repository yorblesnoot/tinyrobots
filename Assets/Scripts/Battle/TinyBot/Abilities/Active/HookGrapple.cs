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
        
        float intervalTime = travelTime / currentTrajectory.Count;
        yield return StartCoroutine(LaunchWithLine(projectile, currentTrajectory, intervalTime));
        bool hitSomething = Vector3.Distance(currentTrajectory[0], currentTrajectory[^1]) < range;
        if (hitSomething)
        {
            if (currentTargets.Count > 0) currentTargets[0].ReceiveHit(damage, Owner.transform.position, currentTrajectory[^1]);
            Vector3 backDirection = currentTrajectory[1] - currentTrajectory[0];
            backDirection.Normalize();
            backDirection *= backDistance;
            Vector3 secondTarget = currentTrajectory[1] - backDirection;
            List<Vector3> secondaryTrajectory = new() { currentTrajectory[0], secondTarget };
            yield return StartCoroutine(LaunchWithLine(Owner.gameObject, secondaryTrajectory, intervalTime));
        }
        else
        {
            List<Vector3> reverseTrajectory = new() { projectile.transform.position, baseParent.TransformPoint(baseHookPosition) };
            yield return StartCoroutine(LaunchWithLine(projectile, reverseTrajectory, intervalTime, false));
        }
        
        EndAbility();
        if (hitSomething)
        {
            yield return StartCoroutine(Owner.Fall());
            Pathfinder3D.GeneratePathingTree(Owner.MoveStyle, Owner.transform.position);
        }
    }

    
}

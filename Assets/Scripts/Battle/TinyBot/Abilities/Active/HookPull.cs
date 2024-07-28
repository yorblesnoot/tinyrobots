using System.Collections;
using UnityEngine;

public class HookPull : HookAbility
{
    protected override IEnumerator PerformEffects()
    {
        line.positionCount = 2;
        projectile.transform.SetParent(null, true);

        float intervalTime = travelTime / currentTrajectory.Count;
        yield return StartCoroutine(LaunchWithLine(projectile, currentTrajectory, intervalTime));
        Targetable target = currentTargets != null && currentTargets.Count > 0 ? currentTargets[0] : null;
        if (target != null)
        {
            currentTrajectory[^1] = currentTargets[0].TargetPoint.position;
            currentTargets[0].ReceiveHit(damage, Owner.transform.position, currentTrajectory[^1]);
            Vector3 secondTarget = currentTrajectory[1];
            currentTrajectory = new() { currentTrajectory[^1], secondTarget };
            StartCoroutine(LaunchAlongLine(target.gameObject, travelTime));
            
        }
        yield return StartCoroutine(LaunchWithLine(projectile, currentTrajectory, intervalTime));
        EndAbility();
        if (target != null)
        {
            yield return StartCoroutine(target.Fall());
            Pathfinder3D.GeneratePathingTree(target.MoveStyle, target.transform.position);
        }
    }
}

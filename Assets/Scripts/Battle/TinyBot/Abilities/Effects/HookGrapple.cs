using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HookGrapple : HookAbility
{
    [SerializeField] float backDistance = 2;
    
    public override IEnumerator PerformEffect(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        line.positionCount = 2;
        projectile.transform.SetParent(null, true);
        
        float intervalTime = travelTime / trajectory.Count;
        yield return StartCoroutine(LaunchWithLine(projectile, trajectory, intervalTime));
        bool hitSomething = Vector3.Distance(trajectory[0], trajectory[^1]) < Ability.range;

        if (targets.Count > 0) targets[0].ReceiveHit(Ability.EffectMagnitude, owner, trajectory[^1]);
        Vector3 backDirection = trajectory[1] - trajectory[0];
        backDirection.Normalize();
        backDirection *= backDistance;
        Vector3 secondTarget = trajectory[1] - backDirection;
        List<Vector3> secondaryTrajectory = new() { trajectory[0], secondTarget };
        yield return StartCoroutine(LaunchWithLine(owner.gameObject, secondaryTrajectory, intervalTime));
        
        ResetHook();
        if (hitSomething)
        {
            yield return StartCoroutine(owner.Fall());
            Pathfinder3D.GeneratePathingTree(owner.MoveStyle, owner.transform.position);
        }
    }
}
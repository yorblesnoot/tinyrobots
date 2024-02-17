using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LegJump : ParabolicAbility
{
    [SerializeField] float intervalTime = .2f;
    [SerializeField] Animator animator;
    protected override IEnumerator PerformEffects()
    {
        Vector3 lookTarget = targetTrajectory.Last();
        lookTarget.y = transform.position.y;
        animator.Play("Hop");
        yield return new WaitForSeconds(.4f);

        foreach (Vector3 point in targetTrajectory)
        {
            yield return StartCoroutine(owner.gameObject.LerpTo(point, intervalTime));
        }
        animator.Play("Idle");
        yield return StartCoroutine(owner.PrimaryMovement.NeutralStance());
        Pathfinder3D.GeneratePathingTree(MoveStyle.WALK, Vector3Int.RoundToInt(owner.transform.position));
    }

    public override bool IsUsable(Vector3 sourcePosition)
    {
        if (Pathfinder3D.GetLandingPointBy(targetTrajectory[^1], MoveStyle.WALK, out Vector3Int nodeTarget))
        {
            targetTrajectory[^1] = nodeTarget;
            return true;
        }
        return false;
    }
}

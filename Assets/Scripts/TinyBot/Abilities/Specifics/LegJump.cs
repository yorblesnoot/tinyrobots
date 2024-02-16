using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LegJump : ParabolicAbility
{
    [SerializeField] float intervalTime = .2f;
    [SerializeField] Animator animator;
    public override IEnumerator ExecuteAbility(Vector3 target)
    {
        Vector3[] parabola = GetTrajectory(transform.position, target);
        Vector3 lookTarget = target;
        lookTarget.y = transform.position.y;
        animator.Play("Hop");
        yield return new WaitForSeconds(.4f);

        foreach (Vector3 point in parabola)
        {
            yield return StartCoroutine(owner.gameObject.LerpTo(point, intervalTime));
        }
        animator.Play("Idle");
        yield return StartCoroutine(owner.PrimaryMovement.NeutralStance());
        Pathfinder3D.GeneratePathingTree(MoveStyle.WALK, Vector3Int.RoundToInt(target));
    }

    public override bool ConfirmAbility(Vector3 target, out Vector3 confirmedTarget)
    {
        confirmedTarget = target;
        Vector3[] parabola = GetTrajectory(transform.position, target);
        List<Vector3> scannedParabola = CastAlongPoints(parabola, LayerMask.GetMask("Terrain"), out _);
        if (scannedParabola.Count < parabola.Length)
        {
            return false;
        }
        Vector3 finalTarget = scannedParabola.Last();
        if (Pathfinder3D.GetLandingPointBy(finalTarget, MoveStyle.WALK, out Vector3Int nodeTarget))
        {
            confirmedTarget = nodeTarget;
            return true;
        }
        return false;
    }
}

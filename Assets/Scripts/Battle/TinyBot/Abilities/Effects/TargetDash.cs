using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetDash : AbilityEffect
{
    [SerializeField] float intervalTime = .2f;

    /*public override bool IsUsable(Vector3 targetPosition)
    {
        if (Pathfinder3D.GetLandingPointBy(CurrentTrajectory[^1], Owner.MoveStyle, out Vector3Int nodeTarget))
        {
            CurrentTrajectory[^1] = nodeTarget;
            return true;
        }
        return false;
    }*/

    public override IEnumerator PerformEffect(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        bool landingPoint = Pathfinder3D.GetLandingPointBy(trajectory[^1], owner.MoveStyle, out Vector3Int target);
        if (landingPoint)
        {
            Quaternion finalRotation = owner.PrimaryMovement.GetRotationAtPosition(target);
            Tween.Rotation(owner.transform, finalRotation, intervalTime * trajectory.Count);
        }
        for (int i = 1; i < trajectory.Count; i++)
        {
            Vector3 point = trajectory[i];
            yield return StartCoroutine(owner.gameObject.LerpTo(point, intervalTime));
        }
        if (!landingPoint) yield return StartCoroutine(owner.Fall());
        else StartCoroutine(owner.PrimaryMovement.NeutralStance());
        Pathfinder3D.GeneratePathingTree(owner.MoveStyle, owner.transform.position);
    }
}

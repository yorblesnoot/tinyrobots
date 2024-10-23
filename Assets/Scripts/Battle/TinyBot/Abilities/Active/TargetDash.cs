using PrimeTween;
using System.Collections;
using System.Linq;
using UnityEngine;

public class TargetDash : ActiveAbility
{
    [SerializeField] float intervalTime = .2f;
    protected override IEnumerator PerformEffects()
    {
        bool landingPoint = Pathfinder3D.GetLandingPointBy(CurrentTrajectory[^1], Owner.MoveStyle, out Vector3Int target);
        if (landingPoint)
        {
            Quaternion finalRotation = Owner.PrimaryMovement.GetRotationAtPosition(target);
            Tween.Rotation(Owner.transform, finalRotation, intervalTime * CurrentTrajectory.Count);
        }
        for (int i = 1; i < CurrentTrajectory.Count; i++)
        {
            Vector3 point = CurrentTrajectory[i];
            yield return StartCoroutine(Owner.gameObject.LerpTo(point, intervalTime));
        }
        if (!landingPoint) yield return StartCoroutine(Owner.Fall());
        else StartCoroutine(Owner.PrimaryMovement.NeutralStance());
        Pathfinder3D.GeneratePathingTree(Owner.MoveStyle, Owner.transform.position);
    }

    public override bool IsUsable(Vector3 targetPosition)
    {
        if (Pathfinder3D.GetLandingPointBy(CurrentTrajectory[^1], Owner.MoveStyle, out Vector3Int nodeTarget))
        {
            CurrentTrajectory[^1] = nodeTarget;
            return true;
        }
        return false;
    }
}

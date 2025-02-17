using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetDash : AbilityEffect
{
    [SerializeField] float intervalTime = .2f;
    [SerializeField] bool fallWithMomentum = true;

    public override IEnumerator PerformEffect(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        bool landingPoint = Pathfinder3D.GetLandingPointBy(trajectory[^1], owner.MoveStyle, out Vector3Int target);
        if (landingPoint)
        {
            Quaternion finalRotation = owner.PrimaryMovement.GetRotationFromFacing(target, owner.transform.forward);
            Tween.Rotation(owner.transform, finalRotation, intervalTime * trajectory.Count);
        }
        for (int i = 1; i < trajectory.Count; i++)
        {
            Vector3 point = trajectory[i];
            yield return StartCoroutine(owner.gameObject.LerpTo(point, intervalTime));
        }
        if (!landingPoint)
        {
            Vector3 velocity = fallWithMomentum ? (trajectory[^1] - trajectory[^2]) / intervalTime : Vector3.zero;
            yield return StartCoroutine(owner.Fall(velocity));
        }
        else owner.PrimaryMovement.LandingStance();
        Pathfinder3D.GeneratePathingTree(owner.MoveStyle, owner.transform.position);
    }
}

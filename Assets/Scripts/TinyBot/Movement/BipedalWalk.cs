using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BipedalWalk : LegMovement
{
    public override IEnumerator PathToPoint(List<Vector3> path)
    {
        foreach (var target in path)
        {
            yield return StartCoroutine(InterpolatePositionAndRotation(Owner.transform, target));
        }
        StartCoroutine(NeutralStance());

    }
    public override void SpawnOrientation()
    {
        StartCoroutine(NeutralStance());
    }
    protected override void InitializeParameters()
    {
        PreferredCursor = CursorType.GROUND;
        Style = MoveStyle.WALK;
    }
    protected override Vector3 GetLimbTarget(Anchor anchor, bool goToNeutral, Vector3 localStartPosition)
    {
        Vector3 direction = transform.forward;
        direction.Normalize();
        Vector3 initialPosition = anchor.localBasePosition + (goToNeutral ? Vector3.zero : direction * anchorZoneRadius * 2);
        Vector3 rayPosition = initialPosition;
        rayPosition = legModel.TransformPoint(rayPosition);
        rayPosition.y += anchorUpwardLimit;
        
        Ray ray = new(rayPosition, Vector3.down);
        Vector3 finalPosition = initialPosition;
        if (Physics.Raycast(ray, out var hitInfo, anchorDownwardLength, LayerMask.GetMask("Terrain")))
        {
            finalPosition = hitInfo.point;
            finalPosition = legModel.InverseTransformPoint(finalPosition);
        }

        return finalPosition;
    }

    protected override Quaternion GetRotationAtPosition(Vector3 moveTarget)
    {
        moveTarget.y = transform.position.y;
        Quaternion targetRotation = Quaternion.LookRotation(moveTarget - transform.position);
        return targetRotation;
    }

    /*protected override Quaternion GetRotationAtPosition(Vector3 moveTarget)
    {
        Vector3 targetNormal = GetMeshFacingAt(moveTarget);
        targetNormal = Vector3.Slerp(targetNormal, Vector3.up, .9f);
        Vector3 lookTarget = moveTarget + targetNormal * lookHeightModifier;
        Quaternion targetRotation = Quaternion.LookRotation(lookTarget - transform.position, targetNormal);
        return targetRotation;
    }*/
}
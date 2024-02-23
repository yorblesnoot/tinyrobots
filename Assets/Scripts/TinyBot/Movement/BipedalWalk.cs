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
    [SerializeField] float forwardStep = 2f;
    protected override Vector3 GetLimbTarget(Anchor anchor, bool goToNeutral, Vector3 localStartPosition)
    {
        Vector3 direction = Owner.transform.forward;
        direction.Normalize();
        Vector3 initialPosition = anchor.localBasePosition + (goToNeutral ? Vector3.zero : forwardStep * direction);
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

    protected override void GluePosition(Anchor anchor)
    {
        if (anchor.stepping) return;
        anchor.ikTarget.position = anchor.gluedWorldPosition;
        anchor.distanceFromBase = anchor.localBasePosition.z - anchor.ikTarget.localPosition.z;
        Debug.Log(anchor.distanceFromBase);
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
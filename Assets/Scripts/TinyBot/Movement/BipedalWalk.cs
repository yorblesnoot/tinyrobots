using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BipedalWalk : LegMovement
{
    protected override void InitializeParameters()
    {
        PreferredCursor = CursorType.GROUND;
        Style = MoveStyle.WALK;
    }
    [SerializeField] float forwardStep = 2f;
    protected override Vector3 GetLimbTarget(Anchor anchor, bool goToNeutral, Vector3 localStartPosition)
    {
        Vector3 direction = Owner.transform.forward;
        direction = Owner.transform.InverseTransformDirection(direction);
        Vector3 initialPosition = anchor.localBasePosition + (goToNeutral ? Vector3.zero : forwardStep * direction);
        Vector3 rayPosition = initialPosition;
        rayPosition = legModel.transform.TransformPoint(rayPosition);
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
    }
}
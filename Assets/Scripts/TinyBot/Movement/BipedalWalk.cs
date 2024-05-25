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

    [SerializeField] float zoneLength = 3f;
    [SerializeField] float zoneWidth = 1f;
    protected override Vector3 GetLimbTarget(Anchor anchor, bool goToNeutral, Vector3 localStartPosition)
    {
        Vector3 direction = Owner.transform.forward;
        direction = Owner.transform.InverseTransformDirection(direction);
        Vector3 initialPosition = anchor.localBasePosition + (goToNeutral ? Vector3.zero : zoneLength * direction);
        Vector3 rayPosition = legModel.transform.TransformPoint(initialPosition);
        rayPosition.y += anchorUpwardLimit;
        
        Ray ray = new(rayPosition, Vector3.down);
        if (Physics.Raycast(ray, out var hitInfo, anchorDownwardLength, LayerMask.GetMask("Terrain")))
        {
            return legModel.InverseTransformPoint(hitInfo.point);
        }
        else
            return default;
    }

    protected override float LegDistanceFromDeadZone(Anchor anchor)
    {
        
        Vector3 basePosition = anchor.localBasePosition;
        Vector3 currentPosition = anchor.ikTarget.localPosition;
        float lengthOffset = Mathf.Abs(basePosition.z - currentPosition.z);
        float widthOffset = Mathf.Abs(basePosition.x - currentPosition.x);
        float lengthScore = Mathf.Clamp(lengthOffset - zoneLength, 0, float.PositiveInfinity);
        float widthScore = Mathf.Clamp(widthOffset - zoneWidth, 0, float.PositiveInfinity);
        float score = lengthScore + widthScore;
        return score;
    }
}
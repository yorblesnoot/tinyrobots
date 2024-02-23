using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderCrawl : LegMovement
{
    [SerializeField][Range(0f, 1f)] float raycastInTilt;
    [SerializeField] float anchorInwardLength;

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
        Vector3 normal = GetMeshFacingAt(Owner.transform.position);
        Vector3 displace = Vector3.Cross(normal, Vector3.up);
        //look position and normal cant be the same?
        Owner.transform.rotation = GetRotationAtPosition(Owner.transform.position + displace);
        StartCoroutine(NeutralStance());
    }
    protected override void InitializeParameters()
    {
        PreferredCursor = CursorType.GROUND;
        Style = MoveStyle.CRAWL;
    }
    protected override Vector3 GetLimbTarget(Anchor anchor, bool goToNeutral, Vector3 localStartPosition)
    {
        Vector3 direction = anchor.localBasePosition - localStartPosition;
        direction.Normalize();
        Vector3 initialPosition = anchor.localBasePosition + (goToNeutral ? Vector3.zero : direction * anchorZoneRadius);
        Vector3 rayPosition = initialPosition;


        rayPosition.y += anchorUpwardLimit;
        Vector3 secondaryRayPosition = rayPosition;

        rayPosition = legModel.TransformPoint(rayPosition);
        secondaryRayPosition = legModel.TransformPoint(secondaryRayPosition);
        Vector3 centerDirection = transform.position - secondaryRayPosition;
        centerDirection.Normalize();
        Vector3 finalDirection = Vector3.Slerp(-Owner.transform.up, centerDirection, raycastInTilt);

        Ray ray = new(rayPosition, finalDirection);
        Ray secondRay = new(secondaryRayPosition, centerDirection);
        Vector3 finalPosition = initialPosition;
        if (Physics.Raycast(ray, out var hitInfo, anchorDownwardLength, LayerMask.GetMask("Terrain"))
            || Physics.Raycast(secondRay, out hitInfo, anchorInwardLength, LayerMask.GetMask("Terrain")))
        {
            finalPosition = hitInfo.point;
            finalPosition = legModel.InverseTransformPoint(finalPosition);
        }

        return finalPosition;
    }
    
    
    protected override Quaternion GetRotationAtPosition(Vector3 moveTarget)
    {
        Vector3 targetNormal = GetMeshFacingAt(moveTarget);
        Vector3 lookTarget = moveTarget + targetNormal * lookHeightModifier;
        Quaternion targetRotation = Quaternion.LookRotation(lookTarget - transform.position, targetNormal);
        return targetRotation;
    }
    
}
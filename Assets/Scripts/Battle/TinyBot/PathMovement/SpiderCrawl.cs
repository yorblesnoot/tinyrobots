using System;
using UnityEngine;

public class SpiderCrawl : LegMovement
{
    [SerializeField][Range(0f, 1f)] float firstCastTilt;
    [SerializeField][Range(0f, 1f)] float secondCastTilt;
    [SerializeField] float secondCastLength;
    [SerializeField] float secondCastHeight = -1;
    
    
    public override void SpawnOrientation()
    {
        Vector3 normal = GetMeshNormalAt(Owner.transform.position);
        Vector3 centerDirection = GetCenterColumn() - transform.position;
        Vector3 facing = Vector3.Cross(normal, centerDirection);
        //look position and normal cant be the same?
        Owner.transform.rotation = Quaternion.LookRotation(facing, normal);
        StartCoroutine(NeutralStance());
    }
    protected override void InitializeParameters()
    {
        Style = MoveStyle.CRAWL;
    }
    protected override Vector3 GetLimbTarget(Anchor anchor, bool goToNeutral, Vector3 localStartPosition)
    {
        Vector3 targetDirection = anchor.localBasePosition - localStartPosition;
        targetDirection.Normalize();

        Vector3 worldForward = goToNeutral ? Vector3.zero : transform.forward * forwardBias;
        Vector3 localForward = anchor.ikTarget.InverseTransformDirection(worldForward);
        Vector3 firstRaySource = anchor.localBasePosition + localForward;


        firstRaySource.y += anchorUpwardLimit;
        Vector3 secondRaySource = firstRaySource;
        secondRaySource.y += secondCastHeight;

        firstRaySource = legModel.TransformPoint(firstRaySource) + worldForward;
        secondRaySource = legModel.TransformPoint(secondRaySource) + worldForward;
        Vector3 centerDirection = transform.position - firstRaySource;
        centerDirection.Normalize();
        Vector3 firstRayDirection = Vector3.Slerp(-Owner.transform.up, centerDirection, firstCastTilt);
        Vector3 secondRayDirection = Vector3.Slerp(-Owner.transform.up, centerDirection, secondCastTilt);

        Ray firstRay = new(firstRaySource, firstRayDirection);
        Ray secondRay = new(secondRaySource, secondRayDirection);

        Debug.DrawRay(firstRaySource, firstRayDirection * anchorDownwardLength, Color.green, 20);
        Debug.DrawRay(secondRaySource, secondRayDirection * secondCastLength, Color.red, 20);

        if (Physics.Raycast(firstRay, out var hitInfo, anchorDownwardLength, LayerMask.GetMask("Terrain"))
            || Physics.Raycast(secondRay, out hitInfo, secondCastLength, LayerMask.GetMask("Terrain")))
        {
            return hitInfo.point;
        }
        else
        {
            return default;
        }
    }
    
    public override Quaternion GetRotationAtPosition(Vector3 moveTarget)
    {
        Vector3 targetNormal = GetMeshNormalAt(moveTarget);
        Vector3 lookTarget = moveTarget + targetNormal * lookHeightModifier;
        Quaternion targetRotation = Quaternion.LookRotation(lookTarget - transform.position, targetNormal);
        return targetRotation;
    }
}
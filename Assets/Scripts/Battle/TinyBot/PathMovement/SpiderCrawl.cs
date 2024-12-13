using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderCrawl : LegMovement
{
    [SerializeField][Range(0f, 1f)] float firstCastTilt;
    [SerializeField][Range(0f, 1f)] float secondCastTilt;
    [SerializeField] float secondCastLength;
    [SerializeField] float secondCastHeight = -1;
    
    

    public override void SpawnOrientation()
    {
        Vector3 normal = Pathfinder3D.GetCrawlOrientation(Owner.transform.position);
        Vector3 centerDirection = GetCenterColumn() - transform.position;
        Vector3 facing = Vector3.Cross(normal, centerDirection);
        //look position and normal cant be the same?
        Owner.transform.rotation = Quaternion.LookRotation(facing, normal);
        
        InstantNeutral();
    }

    public override List<Vector3> SanitizePath(List<Vector3> path)
    {
        path = StandardizeHeight(path);
        return ShortcutPath(path);
    }

    List<Vector3> StandardizeHeight(List<Vector3> path)
    {
        List<Vector3> newPath = new();
        foreach(var point in path)
        {
            Vector3 direction = -Pathfinder3D.GetCrawlOrientation(point);
            if (!Physics.Raycast(point, direction, out RaycastHit hit, locomotionHeight * 2, TerrainMask))
            {
                newPath.Add(point);
                continue;
            }
            Vector3 cleanPoint = hit.point + -direction * locomotionHeight;
            newPath.Add(cleanPoint);
        }
        return newPath;
    }

    protected override IEnumerator InterpolatePositionAndRotation(Transform unit, Vector3 target)
    {
        Quaternion startRotation = unit.rotation;
        Quaternion targetRotation = GetRotationAtPosition(target);

        Vector3 startPosition = unit.position;
        float timeElapsed = 0;
        float pathStepDuration = Vector3.Distance(unit.transform.position, target) / (MoveSpeed * SpeedMultiplier);
        Vector3 targetLegDirection = targetRotation * Vector3.forward;
        while (timeElapsed < pathStepDuration)
        {
            unit.SetPositionAndRotation(Vector3.Lerp(startPosition, target, timeElapsed / pathStepDuration), 
                Quaternion.Slerp(startRotation, targetRotation, timeElapsed / pathStepDuration));
            timeElapsed += Time.deltaTime;

            AnimateToOrientation(targetLegDirection);
            yield return null;
        }
    }
    protected override Vector3 GetLimbTarget(Anchor anchor, Vector3 legDirection)
    {
        Vector3 worldForward = legDirection * forwardBias;
        Vector3 localForward = anchor.ikTarget.InverseTransformDirection(worldForward);
        Vector3 firstRaySource = anchor.LocalBasePosition + localForward;
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
        Debug.LogWarning("GetRotationAtPosition in SpiderCrawl might have a problem");
        Vector3 targetNormal = Pathfinder3D.GetCrawlOrientation(moveTarget);
        Vector3 lookTarget = moveTarget + targetNormal * locomotionHeight;
        Quaternion targetRotation = Quaternion.LookRotation(lookTarget - transform.position, targetNormal);
        return targetRotation;
    }
}
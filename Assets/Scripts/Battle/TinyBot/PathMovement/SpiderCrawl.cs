using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderCrawl : LegMovement
{
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
            if (!Physics.Raycast(point, direction, out RaycastHit hit, LocomotionHeight * 2, TerrainMask))
            {
                newPath.Add(point);
                continue;
            }
            Vector3 cleanPoint = hit.point;
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

    public override Quaternion GetRotationAtPosition(Vector3 moveTarget)
    {
        //check for issues
        Vector3 targetNormal = Pathfinder3D.GetCrawlOrientation(moveTarget);
        Vector3 lookTarget = moveTarget + targetNormal * Owner.TargetPoint.localPosition.magnitude;
        Quaternion targetRotation = Quaternion.LookRotation(moveTarget - transform.position, targetNormal);
        return targetRotation;
    }
}
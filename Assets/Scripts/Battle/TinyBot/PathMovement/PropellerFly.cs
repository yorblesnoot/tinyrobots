using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PropellerFly : PrimaryMovement
{
    int sanitizeMask;
    private void Awake()
    {
        Style = MoveStyle.FLY;
        sanitizeMask = LayerMask.GetMask("Terrain", "Default");
    }

    public override Quaternion GetRotationAtPosition(Vector3 moveTarget)
    {
        moveTarget.y = transform.position.y;
        Quaternion targetRotation = moveTarget == transform.position ? transform.rotation : Quaternion.LookRotation(moveTarget - transform.position);
        return targetRotation;
    }

    public override IEnumerator NeutralStance()
    {
        yield return null;
    }

    readonly float overlap = .01f;
    public override List<Vector3> SanitizePath(List<Vector3> path)
    {
        int pathIndex = 0;
        List<Vector3> cleanPath = new() { path[0] };
        while(pathIndex < path.Count)
        {
            cleanPath.Add(path[pathIndex]);
            pathIndex = GetNextPoint(path, pathIndex);
        }

        return cleanPath;
    }

    int GetNextPoint(List<Vector3> path, int startIndex)
    {
        int leapSize = 1;
        while (StraightPathOpen(path, startIndex, leapSize + 1))
        {
            leapSize++;
        }
        return startIndex + leapSize;
    }

    bool StraightPathOpen(List<Vector3> path, int pathIndex, int leapSize)
    {
        if (pathIndex + leapSize >= path.Count) return false;
        Vector3 originPoint = path[pathIndex];
        Vector3 testPoint = path[pathIndex + leapSize];
        Vector3 direction = testPoint - originPoint;
        float distance = Vector3.Distance(originPoint, testPoint) + overlap;

        return !Physics.Raycast(path[pathIndex], direction, distance, sanitizeMask);
    }
}

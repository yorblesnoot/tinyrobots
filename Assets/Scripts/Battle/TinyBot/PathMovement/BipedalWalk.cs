using System.Collections.Generic;
using UnityEngine;

public class BipedalWalk : LegMovement
{
    List<Vector3> sanitizationPositions;
    readonly float sanOffset = 1.5f;
    readonly float pathHeight = .3f;
    readonly float legScanHeight = 2.5f;
    readonly float scanOriginHeight = 1;

    protected override void InitializeParameters()
    {
        base.InitializeParameters();
        sanitizationPositions = new() { Vector3.zero, Vector3.left * sanOffset, Vector3.right * sanOffset, Vector3.forward * sanOffset, Vector3.back * sanOffset };
    }

    public override List<Vector3> SanitizePath(List<Vector3> path)
    {
        //TODO: something here is causing lots of assertion errors
        path = ShortcutPath(path);
        path = AvoidEmptySpace(path);
        return path;
    }

    private List<Vector3> AvoidEmptySpace(List<Vector3> path)
    {
        List<Vector3> newPath = new();
        Vector3 castDirection = Vector3.down;
        foreach (var point in path)
        {
            Vector3 target = point - castDirection * scanOriginHeight;
            List<Vector3> hitpoints = new();
            foreach (var offset in sanitizationPositions)
            {
                if (Physics.Raycast(target + offset, castDirection, out RaycastHit hit, legScanHeight, TerrainMask)) hitpoints.Add(hit.point);
            }
            Vector3 newPoint = hitpoints.Count > 0 ? hitpoints.Average() + -castDirection * pathHeight : point;
            newPath.Add(newPoint);
        }
        return newPath;
    }

    

    protected override Vector3 GetLimbTarget(Anchor anchor, Vector3 legDirection)
    {
        Vector3 initialPosition = anchor.LocalBasePosition + (anchorZoneRadius + forwardBias) * legModel.transform.InverseTransformDirection(legDirection);
        Vector3 rayPosition = legModel.transform.TransformPoint(initialPosition);
        rayPosition.y += anchorUpwardLimit;

        Debug.DrawLine(rayPosition, rayPosition + Vector3.down * anchorDownwardLength, Color.green, 30);
        
        Ray ray = new(rayPosition, Vector3.down);
        if (Physics.Raycast(ray, out var hitInfo, anchorDownwardLength, LayerMask.GetMask("Terrain")))
        {
            return hitInfo.point;
        }
        else return default;
    }
}
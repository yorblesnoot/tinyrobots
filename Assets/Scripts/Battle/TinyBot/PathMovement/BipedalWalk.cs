using UnityEngine;

public class BipedalWalk : LegMovement
{
    protected override void InitializeParameters()
    {
        Style = MoveStyle.WALK;
    }

    protected override Vector3 GetLimbTarget(Anchor anchor, bool goToNeutral, Vector3 localStartPosition)
    {
        Vector3 initialPosition = anchor.localBasePosition + (goToNeutral ? Vector3.zero : (anchorZoneRadius + forwardBias) * Vector3.forward);
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
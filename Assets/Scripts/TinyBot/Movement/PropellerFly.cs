using System.Collections;
using UnityEngine;

public class PropellerFly : PrimaryMovement
{
    private void Awake()
    {
        PreferredCursor = CursorType.AIR;
        Style = MoveStyle.FLY;
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
}

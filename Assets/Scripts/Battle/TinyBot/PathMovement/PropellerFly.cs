using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PropellerFly : PrimaryMovement
{
    
    private void Awake()
    {
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

    
    public override List<Vector3> SanitizePath(List<Vector3> path)
    {
        return ShortcutPath(path);
    }

    

    protected override void InstantNeutral() { }
}

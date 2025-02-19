using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropellerFly : PrimaryMovement
{
    public override float LocomotionHeight => locomotionHeight;
    [SerializeField] float locomotionHeight;

    protected override void AwakeInitialize()
    {
        base.AwakeInitialize();
        Style = MoveStyle.FLY;
    }

    public override Quaternion GetRotationFromFacing(Vector3 position, Vector3 facing)
    {
        if(facing.normalized == Vector3.up) return Owner.transform.rotation;
        return base.GetRotationFromFacing(position, facing);
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

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PropellerFly : PrimaryMovement
{
    public override float LocomotionHeight => locomotionHeight;
    [SerializeField] float locomotionHeight;

    private void Awake()
    {
        Style = MoveStyle.FLY;
    }

    public override Quaternion GetRotationFromFacing(Vector3 position, Vector3 facing)
    {
        position.y = transform.position.y;
        Quaternion targetRotation = position == transform.position ? transform.rotation : Quaternion.LookRotation(facing);
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

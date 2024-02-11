using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropellerFly : PrimaryMovement
{
    private void Awake()
    {
        PreferredCursor = CursorType.AIR;
        MoveStyle = MoveStyle.FLY;
    }
    public override IEnumerator PathToPoint(TinyBot user, List<Vector3> path)
    {
        foreach (var target in path)
        {
            Vector3 flatPosition = target;
            flatPosition.y = user.transform.position.y;
            user.transform.LookAt(flatPosition);
            yield return StartCoroutine(user.gameObject.LerpTo(target, .1f));
        }
    }

    public override void SpawnOrientation(Transform unit)
    {
        
    }

    public override IEnumerator RotateInPlace()
    {
        yield return null;
    }
}

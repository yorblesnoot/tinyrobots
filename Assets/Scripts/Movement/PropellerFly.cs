using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropellerFly : PrimaryMovement
{
    private void Awake()
    {
        PreferredCursor = CursorType.AIR;
        Style = MoveStyle.FLY;
    }
    public override IEnumerator PathToPoint(List<Vector3> path)
    {
        foreach (var target in path)
        {
            Vector3 flatPosition = target;
            flatPosition.y = Owner.transform.position.y;
            Owner.transform.LookAt(flatPosition);
            yield return StartCoroutine(Owner.gameObject.LerpTo(target, .1f));
        }
    }

    public override void SpawnOrientation()
    {
        
    }

    public override IEnumerator NeutralStance()
    {
        yield return null;
    }
}

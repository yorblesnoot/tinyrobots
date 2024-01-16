using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropellerMove : Ability
{
    public override void ActivateAbility(TinyBot user, Vector3 target)
    {
        var path = Pathfinder3D.FindVectorPath(Vector3Int.RoundToInt(user.transform.position), Vector3Int.RoundToInt(target));
        if(path == null) return;
        StartCoroutine(FlyPath(user, path));
    }

    IEnumerator FlyPath(TinyBot user, List<Vector3> path)
    {
        foreach (var target in path)
        {
            Vector3 flatPosition = target;
            flatPosition.y = user.transform.position.y;
            user.transform.LookAt(flatPosition);
            yield return StartCoroutine(user.gameObject.LerpTo(target, .2f));
        }
    }
}

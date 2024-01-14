using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropellerMove : Ability
{
    public override void ActivateAbility(TinyBot user, Vector3 target)
    {
        Vector3 flatPosition = target;
        flatPosition.y = user.transform.position.y;
        user.transform.LookAt(flatPosition);
        StartCoroutine(user.gameObject.LerpTo(target, 1));
    }
}

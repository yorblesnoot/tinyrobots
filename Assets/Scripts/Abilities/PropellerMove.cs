using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropellerMove : Ability
{
    public override void ActivateAbility(TinyBot user, Vector3 target)
    {
        StartCoroutine(user.gameObject.LerpTo(target, 1));
    }
}

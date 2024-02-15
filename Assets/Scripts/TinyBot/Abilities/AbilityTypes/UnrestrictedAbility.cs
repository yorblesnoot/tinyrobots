using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UnrestrictedAbility : Ability
{
    protected override void FollowEntity(GameObject target)
    {
        return;
    }
}

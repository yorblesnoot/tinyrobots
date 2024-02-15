using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UnrestrictedAbility : Ability
{
    protected override void AimAt(GameObject target)
    {
        return;
    }
}

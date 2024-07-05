using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SelfAbility : Ability
{
    public override List<TinyBot> AimAt(GameObject target, Vector3 sourcePosition, bool aiMode = false)
    {
        return new List<TinyBot>() { Owner };
    }
}

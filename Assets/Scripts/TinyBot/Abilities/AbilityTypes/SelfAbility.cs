using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SelfAbility : ActiveAbility
{
    public override List<Targetable> AimAt(GameObject target, Vector3 sourcePosition, bool aiMode = false)
    {
        return new List<Targetable>() { Owner };
    }
}

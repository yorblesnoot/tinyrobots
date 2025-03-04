using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleMeta : AbilityEffect
{
    [SerializeField] MetaAbility metaAbility;
    public override IEnumerator PerformEffect(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        metaAbility.SetActiveAlternate();
        yield break;
    }
}

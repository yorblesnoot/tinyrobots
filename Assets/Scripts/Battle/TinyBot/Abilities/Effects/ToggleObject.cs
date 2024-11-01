using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleObject : AbilityEffect
{
    [SerializeField] GameObject target;
    [SerializeField] bool on;
    public override IEnumerator PerformEffect(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        target.SetActive(on);
        yield break;
    }
}

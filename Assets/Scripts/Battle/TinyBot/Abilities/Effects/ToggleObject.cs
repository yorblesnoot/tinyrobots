using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleObject : AbilityEffect
{
    enum Placement
    {
        BASE,
        BOT
    }
    [SerializeField] GameObject target;
    [SerializeField] bool on;
    [SerializeField] Placement placement;
    public override IEnumerator PerformEffect(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        target.SetActive(on);
        if(placement == Placement.BOT)
        {
            target.transform.SetParent(owner.transform, false);
            target.transform.localPosition = Vector3.zero;
        }
        yield break;
    }
}

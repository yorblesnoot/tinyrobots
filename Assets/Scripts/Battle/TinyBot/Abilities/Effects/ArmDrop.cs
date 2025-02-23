using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmDrop : AbilityEffect
{
    [SerializeField] ArmGrab armGrab;

    public override IEnumerator PerformEffect(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        if (armGrab.Grabbed == null) yield break;
        StartCoroutine(armGrab.Grabbed.Fall());
        DropGrabbed();
    }

    public void DropGrabbed()
    {
        armGrab.FreezeIdleOfGrabbed(false);
        armGrab.Grabbed.ToggleActiveLayer(false);
        armGrab.Grabbed.transform.SetParent(null, true);
        armGrab.ToggleAbilityLock(Ability.Owner, false);
        armGrab.Grabbed = null;
    }
}

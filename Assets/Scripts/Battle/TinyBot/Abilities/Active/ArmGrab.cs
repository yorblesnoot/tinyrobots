using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArmGrab : ActiveAbility
{
    [SerializeField] ArmThrow armThrow;
    [HideInInspector] public Targetable Grabbed;
    protected override IEnumerator PerformEffects()
    {
        Grabbed = CurrentTargets[0];
        Grabbed.ToggleActiveLayer(true);
        Pathfinder3D.EvaluateNodeOccupancy(Owner.transform.position);
        Grabbed.transform.SetParent(emissionPoint.transform, true);
        Owner.Stats.Current[StatType.MOVEMENT] /= 2;
        if (Owner.Allegiance == Allegiance.PLAYER) TurnResourceCounter.Update?.Invoke();
        ToggleAbilityLock();
        yield return null;
    }
    void ToggleAbilityLock(bool on = true)
    {
        foreach (ActiveAbility ability in Owner.ActiveAbilities)
        {
            ability.locked = on;
        }
        armThrow.locked = !on;
    }

    public override bool IsUsable(Vector3 targetPosition)
    {
        if(CurrentTargets.Count == 0) return false;
        return true;
    }

    public override void EndAbility()
    {
        base.EndAbility();
        if (Grabbed == null) return;
        StartCoroutine(Grabbed.Fall());
        EndGrab();
    }

    public void EndGrab()
    {
        Grabbed.ToggleActiveLayer(false);
        Grabbed.transform.SetParent(null, true);
        Grabbed = null;
        ToggleAbilityLock(false);
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArmGrab : AbilityEffect
{
    [SerializeField] ArmThrow armThrow;
    [HideInInspector] public Targetable Grabbed;
    public override IEnumerator PerformEffect(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        Grabbed = targets[0];
        Grabbed.ToggleActiveLayer(true);
        Pathfinder3D.EvaluateNodeOccupancy(owner.transform.position);
        Grabbed.transform.SetParent(Ability.emissionPoint.transform, true);
        owner.Stats.Current[StatType.MOVEMENT] /= 2;
        if (owner.Allegiance == Allegiance.PLAYER) TurnResourceCounter.Update?.Invoke();
        ToggleAbilityLock(owner);
        yield return null;
    }
    void ToggleAbilityLock(TinyBot owner, bool on = true)
    {
        foreach (ActiveAbility ability in owner.ActiveAbilities)
        {
            ability.ProhibitAbility(this, on);
        }
        //armThrow.Ability.ProhibitAbility(this, !on);
    }

    /*
    public override bool IsUsable(Vector3 targetPosition)
    {
        if(CurrentTargets.Count == 0) return false;
        return true;
    }
    */

    public void EndAbility()
    {
        if (Grabbed == null) return;
        StartCoroutine(Grabbed.Fall());
        EndGrab();
    }
    

    public void EndGrab()
    {
        Grabbed.ToggleActiveLayer(false);
        Grabbed.transform.SetParent(null, true);
        Grabbed = null;
        //ToggleAbilityLock(false);
    }

    
}

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
        Pathfinder3D.EvaluateNodeOccupancy(owner.transform.position, Grabbed.transform.position);
        Grabbed.transform.SetParent(Ability.emissionPoint, true);
        owner.Stats.Current[StatType.MOVEMENT] /= 2;
        ToggleAbilityLock(owner);
        yield return null;
    }
    public void ToggleAbilityLock(TinyBot targetUnit, bool on = true)
    {
        foreach (ActiveAbility ability in targetUnit.ActiveAbilities)
        {
            ability.ProhibitAbility(this, on);
        }
        ActiveAbility active = armThrow.Ability as ActiveAbility;
        active.ProhibitAbility(this, !on);
    }
}

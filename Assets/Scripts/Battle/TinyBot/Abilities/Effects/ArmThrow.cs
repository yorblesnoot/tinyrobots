using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArmThrow : AbilityEffect
{
    [SerializeField] ArmGrab armGrab;
    [SerializeField] ArmDrop armDrop;
    [SerializeField] float thrownAirTime;

    public override void Initialize(Ability ability)
    {
        base.Initialize(ability);
        ActiveAbility active = ability as ActiveAbility;
        active.ProhibitAbility(armGrab);
    }
    

    public override IEnumerator PerformEffect(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        Targetable thrown = armGrab.Grabbed;
        armDrop.DropGrabbed();
        yield return StartCoroutine(ProjectileMovement.LaunchAlongLine(thrown.gameObject, thrownAirTime, trajectory));
        float intervalTime = thrownAirTime / trajectory.Count;
        Vector3 displacement = trajectory[^1] - trajectory[^2];
        yield return StartCoroutine(thrown.Fall(displacement / intervalTime, GetFallBonus(trajectory)));
        Pathfinder3D.EvaluateNodeOccupancy(owner.transform.position);
    }

    float GetFallBonus(List<Vector3> trajectory)
    { 
        float peak = trajectory.Select(point => point.y).Max();
        return peak - trajectory[^1].y;
    }
}

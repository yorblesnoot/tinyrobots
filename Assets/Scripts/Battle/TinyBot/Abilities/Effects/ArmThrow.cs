using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class ArmThrow : AbilityEffect
{
    [SerializeField] ArmGrab armGrab;
    [SerializeField] float thrownAirTime;
    /*
    private void Start()
    {
        ProhibitAbility(armGrab);
    }
    */

    public override IEnumerator PerformEffect(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        Targetable thrown = armGrab.Grabbed;
        armGrab.EndGrab();
        yield return StartCoroutine(ProjectileMovement.LaunchAlongLine(thrown.gameObject, thrownAirTime, trajectory));
        float intervalTime = thrownAirTime / trajectory.Count;
        Vector3 displacement = trajectory[^1] - trajectory[^2];
        armGrab.EndAbility();
        yield return StartCoroutine(thrown.Fall(displacement / intervalTime));
        Pathfinder3D.EvaluateNodeOccupancy(owner.transform.position);
    }
}

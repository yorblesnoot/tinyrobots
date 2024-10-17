using System.Collections;
using UnityEngine;

public class ArmThrow : ActiveAbility
{
    [SerializeField] ArmGrab armGrab;
    [SerializeField] float thrownAirTime;

    private void Start()
    {
        locked = true;
    }

    protected override IEnumerator PerformEffects()
    {
        Targetable thrown = armGrab.Grabbed;
        armGrab.EndGrab();
        yield return StartCoroutine(ProjectileMovement.LaunchAlongLine(thrown.gameObject, thrownAirTime, CurrentTrajectory));
        float intervalTime = thrownAirTime / CurrentTrajectory.Count;
        Vector3 displacement = CurrentTrajectory[^1] - CurrentTrajectory[^2];
        armGrab.EndAbility();
        yield return StartCoroutine(thrown.Fall(displacement / intervalTime));
        Pathfinder3D.EvaluateNodeOccupancy(Owner.transform.position);
    }
}

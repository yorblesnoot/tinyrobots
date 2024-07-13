using System.Collections;
using UnityEngine;

public class ArmThrow : ProjectileAbility
{
    [SerializeField] ArmGrab armGrab;
    [SerializeField] float thrownAirTime;

    private void Start()
    {
        locked = true;
    }

    public void PrepareToThrow()
    {
        Owner.Stats.Current[StatType.MOVEMENT] /= 2;
        Owner.EndedTurn.AddListener(EndAbility);
        if (Owner.Allegiance == Allegiance.PLAYER) TurnResourceCounter.Update?.Invoke();
        foreach (ActiveAbility ability in Owner.ActiveAbilities)
        {
            ability.locked = true;
        }
        locked = false;
    }

    protected override IEnumerator PerformEffects()
    {
        armGrab.EndGrab();
        yield return StartCoroutine(LaunchAlongLine(armGrab.grabbed.gameObject, thrownAirTime));
        EndAbility();
        float intervalTime = thrownAirTime / currentTrajectory.Count;
        Vector3 displacement = currentTrajectory[^1] - currentTrajectory[^2];
        yield return StartCoroutine(armGrab.grabbed.Fall(displacement / intervalTime));
        Pathfinder3D.EvaluateNodeOccupancy(Owner.transform.position);
    }
}

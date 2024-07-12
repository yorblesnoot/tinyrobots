using System.Collections;
using UnityEngine;

public class ArmThrow : ProjectileShot
{
    [SerializeField] ArmGrab armGrab;
    [SerializeField] float thrownAirTime;

    Targetable grabbed;
    private void Start()
    {
        locked = true;
    }

    public void PrepareToThrow(Targetable target)
    {
        grabbed = target;
        Owner.Stats.Current[StatType.MOVEMENT] /= 2;
        Owner.EndedTurn.AddListener(EndAbility);
        if (Owner.Allegiance == Allegiance.PLAYER) TurnResourceCounter.Update?.Invoke();
        foreach (ActiveAbility ability in Owner.Abilities)
        {
            ability.locked = true;
        }
        locked = false;
    }

    public override void EndAbility()
    {
        EndGrab();
        StartCoroutine(grabbed.Fall());
        EndAbility();
    }

    void EndGrab()
    {
        //animator.SetBool("open", true);
        grabbed.ToggleActiveLayer(false);
        grabbed.transform.SetParent(null, true);
        Owner.EndedTurn.RemoveListener(EndAbility);
    }

    protected override IEnumerator PerformEffects()
    {
        EndGrab();
        yield return StartCoroutine(LaunchAlongLine(grabbed.gameObject, thrownAirTime));
        EndAbility();
        float intervalTime = thrownAirTime / currentTrajectory.Count;
        Vector3 displacement = currentTrajectory[^1] - currentTrajectory[^2];
        yield return StartCoroutine(grabbed.Fall(displacement / intervalTime));
        Pathfinder3D.EvaluateNodeOccupancy(Owner.transform.position);
    }
}

using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmThrow : ParabolicAbility
{
    [SerializeField] float thrownAirTime;
    [SerializeField] float windupTime = 1f;
    [SerializeField] float windDistance = 1f;
    [SerializeField] ArmGrab armGrab;
    [SerializeField] Transform ikTarget;
    [SerializeField] Animator animator;

    Targetable grabbed;
    private void Start()
    {
        locked = true;
    }

    public void PrepareToThrow(Targetable target)
    {
        grabbed = target;
        Owner.Stats.Current[StatType.MOVEMENT] /= 2;
        Owner.EndedTurn.AddListener(DropGrabbed);
        if (Owner.Allegiance == Allegiance.PLAYER) TurnResourceCounter.Update?.Invoke();
        foreach (ActiveAbility ability in Owner.Abilities)
        {
            ability.locked = true;
        }
        locked = false;
    }

    public void DropGrabbed()
    {
        EndGrab();
        StartCoroutine(grabbed.Fall());
        NeutralAim();
    }

    void EndGrab()
    {
        animator.SetBool("open", true);
        grabbed.ToggleActiveLayer(false);
        grabbed.transform.SetParent(null, true);
        Owner.EndedTurn.RemoveListener(DropGrabbed);
    }

    protected override IEnumerator PerformEffects()
    {
        Vector3 windDirection = targetTrajectory[0] - targetTrajectory[1];
        windDirection.Normalize();
        Vector3 windTarget = (windDirection * windDistance) + ikTarget.transform.position;

        yield return Tween.Position(ikTarget, endValue: windTarget, duration: windupTime)
            .Chain(Tween.Position(ikTarget, endValue: targetTrajectory[0], duration: windupTime / 2))
            .OnComplete(() => StartCoroutine(FinishThrow()))
            .ToYieldInstruction();
    }

    IEnumerator FinishThrow()
    {
        EndGrab();
        yield return StartCoroutine(LaunchAlongLine(grabbed.gameObject, thrownAirTime));
        NeutralAim();
        float intervalTime = thrownAirTime / targetTrajectory.Count;
        Vector3 displacement = targetTrajectory[^1] - targetTrajectory[^2];
        yield return StartCoroutine(grabbed.Fall(displacement / intervalTime));
        Pathfinder3D.EvaluateNodeOccupancy(Owner.transform.position);
    }

    public override void NeutralAim()
    {
        armGrab.NeutralAim();
    }
}

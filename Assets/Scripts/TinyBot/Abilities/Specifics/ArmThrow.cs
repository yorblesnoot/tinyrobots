using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmThrow : ParabolicAbility
{
    [SerializeField] float thrownAirTime;
    [SerializeField] ArmGrab armGrab;
    
    TinyBot grabbed;
    private void Start()
    {
        locked = true;
    }

    public void PrepareToThrow(TinyBot target)
    {
        grabbed = target;
        Owner.Stats.Current[StatType.MOVEMENT] /= 2;
        Owner.endedTurn.AddListener(DropGrabbed);
        if (Owner.allegiance == Allegiance.PLAYER) TurnResourceCounter.Update?.Invoke();
        foreach (Ability ability in Owner.Abilities)
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
        grabbed.transform.SetParent(null, true);
        Owner.endedTurn.RemoveListener(DropGrabbed);
    }

    protected override IEnumerator PerformEffects()
    {
        EndGrab();
        yield return StartCoroutine(LaunchAlongLine(grabbed.gameObject, thrownAirTime));
        NeutralAim();
        StartCoroutine(grabbed.Fall());
    }

    public override void NeutralAim()
    {
        armGrab.NeutralAim();
    }
}

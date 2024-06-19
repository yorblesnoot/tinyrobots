using System.Collections;
using UnityEngine;

public class MoveBoost : SelfAbility
{
    [SerializeField] Animator animator;
    public override void NeutralAim()
    {
        ToggleThruster(false);
        Owner.endedTurn.RemoveListener(NeutralAim);
        Owner.PrimaryMovement.speedMultiplier = 1;
    }

    protected override IEnumerator PerformEffects()
    {
        ToggleThruster(true);
        Owner.endedTurn.AddListener(NeutralAim);
        Owner.PrimaryMovement.speedMultiplier = 1.5f;
        Owner.Stats.Current[StatType.MOVEMENT] += Owner.Stats.Max[StatType.MOVEMENT];
        if (Owner.allegiance == Allegiance.PLAYER) TurnResourceCounter.Update.Invoke();
        yield break;
    }

    void ToggleThruster(bool setting)
    {
        animator.SetBool("thrusterOn", setting);
    }
}

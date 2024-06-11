using UnityEngine;
using System.Collections;

public class ActionBoost : SelfAbility
{
    [SerializeField] Animator animator;
    [SerializeField] ParticleSystem smokeBurst;
    public override void NeutralAim()
    {
        ToggleGenerator(false);
        Owner.endedTurn.RemoveListener(NeutralAim);
    }

    protected override IEnumerator PerformEffects()
    {
        ToggleGenerator(true);
        smokeBurst.Play();
        Owner.endedTurn.AddListener(NeutralAim);
        Owner.Stats.Current[StatType.ACTION] += 1;
        if (Owner.allegiance == Allegiance.PLAYER) TurnResourceCounter.Update.Invoke();
        yield break;
    }

    void ToggleGenerator(bool setting)
    {
        animator.SetBool("generatorOn", setting);
    }
}
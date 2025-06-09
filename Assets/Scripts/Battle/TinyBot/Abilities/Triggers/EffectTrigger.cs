using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EffectTrigger : TriggerController
{
    public List<AbilityEffect> OutputEffect;

    public override void Initialize(TinyBot owner, Ability ability)
    {
        base.Initialize(owner, ability);
        foreach(var effect in OutputEffect) effect.Initialize(ability);
    }


    protected override void ActivateEffect(TinyBot target)
    {
        Owner.StartCoroutine(ActiveAbility.RunEffectSequence(OutputEffect, Owner, null, new() { alwaysTargetSelf ? Owner : target }));
    }
}

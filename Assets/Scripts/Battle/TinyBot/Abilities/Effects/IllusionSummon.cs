using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IllusionSummon : AbilityEffect
{
    [SerializeField] Material illusionMaterial;
    [SerializeField] BotPalette palette;

    public override IEnumerator PerformEffect(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        BotAssembler.SummonBot(owner.LinkedCore.Bot, owner, trajectory[^1], ConditionBot);
        yield break;
    }

    void ConditionBot(TinyBot summon)
    {
        summon.Stats.Max[StatType.HEALTH] = 1;
        summon.Stats.Current[StatType.HEALTH] = 1;
        summon.DamageCalculator.AddFactor(new MultiplierDamage() { Multiplier = Ability.EffectMagnitude / 100f, Outgoing = true });
        summon.ActiveAbilities.Where(a => a.GetType() == typeof(IllusionSummon)).FirstOrDefault().ProhibitAbility(this);
        foreach (var part in summon.PartModifiers) palette.RecolorPart(part, new Material[] { illusionMaterial });
    }

    
}

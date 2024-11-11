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
        BotAssembler.SummonBot(owner.LinkedCore.Bot.Children[0], owner, trajectory[^1], ConditionBot);
        yield break;
    }

    void ConditionBot(TinyBot summon)
    {
        summon.Stats.Max[StatType.HEALTH] = 1;
        summon.Stats.Current[StatType.HEALTH] = 1;
        summon.Stats.Max[StatType.ACTION] = Ability.Owner.Stats.Max[StatType.ACTION];
        summon.DamageCalculator.AddFactor(new MultiplierDamage() { Multiplier = Ability.EffectMagnitude / 100f, Outgoing = true });
        foreach (var part in summon.PartModifiers) palette.RecolorPart(part, new Material[] { illusionMaterial });
    }

    
}

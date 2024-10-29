using System;
using System.Collections;
using UnityEngine;

public class IllusionSummon : SummonAbility
{
    [SerializeField] Material illusionMaterial;
    [SerializeField] BotPalette palette;
    protected override IEnumerator PerformEffects()
    {
        SummonBot(Owner.LinkedCore.Bot, ConditionBot);
        yield break;
    }

    void ConditionBot(TinyBot summon)
    {
        summon.Stats.Max[StatType.HEALTH] = 1;
        summon.Stats.Current[StatType.HEALTH] = 1;
        summon.DamageCalculator.AddFactor(new MultiplierDamage() { Multiplier = EffectMagnitude / 100f, Outgoing = true });
        foreach (var part in summon.PartModifiers) palette.RecolorPart(part, new Material[] { illusionMaterial });
    }

}

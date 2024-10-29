using System;
using System.Collections;
using System.Linq;
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

    public override void Initialize(TinyBot botUnit)
    {
        base.Initialize(botUnit);
    }

    void ConditionBot(TinyBot summon)
    {
        summon.Stats.Max[StatType.HEALTH] = 1;
        summon.Stats.Current[StatType.HEALTH] = 1;
        summon.DamageCalculator.AddFactor(new MultiplierDamage() { Multiplier = EffectMagnitude / 100f, Outgoing = true });
        summon.ActiveAbilities.Where(a => a.GetType() == typeof(IllusionSummon)).FirstOrDefault().ProhibitAbility(this);
        foreach (var part in summon.PartModifiers) palette.RecolorPart(part, new Material[] { illusionMaterial });
    }

}

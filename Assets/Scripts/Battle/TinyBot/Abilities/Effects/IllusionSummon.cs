using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IllusionSummon : AbilityEffect
{
    [SerializeField] Material illusionMaterial;
    [SerializeField] MultiplierDamage multiplier;
    [SerializeField] int summonLimit = 1;

    public override string Description => " % Damage Dealt";

    List<TinyBot> currentlySummoned = new();

    public override IEnumerator PerformEffect(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        if(currentlySummoned.Count == summonLimit)
        {
            currentlySummoned[0].Die();
            currentlySummoned.RemoveAt(0);
        }
        BotAssembler.SummonBot(owner.LinkedCore.Bot.Children[0], owner, trajectory[^1], ConditionBot);
        yield break;
    }

    void ConditionBot(TinyBot summon)
    {
        currentlySummoned.Add(summon);
        summon.Stats.Max[StatType.HEALTH] = 1;
        summon.Stats.Current[StatType.HEALTH] = 1;
        summon.Stats.Max[StatType.ACTION] = Ability.Owner.Stats.Max[StatType.ACTION];
        summon.Buffs.AddBuff(Ability.Owner, multiplier, Mathf.RoundToInt(Ability.EffectivenessMultiplier * BaseEffectMagnitude));
        foreach (var part in summon.PartModifiers) SceneGlobals.BotPalette.RecolorPart(part, new Material[] { illusionMaterial });
    }

    
}

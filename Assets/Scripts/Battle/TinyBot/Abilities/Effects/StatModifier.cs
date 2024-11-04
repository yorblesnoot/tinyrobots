using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StatModifier : AbilityEffect
{
    public enum BonusMode
    {
        FLAT,
        PERCENTMAX,
        PERCENTCURRENT,
        PERCENTMISSING
    }


    [SerializeField] StatType statType;
    [SerializeField] BonusMode mode;

    static int GetFinalBonus(int bonus, BonusMode mode, TinyBot target, StatType stat)
    {
        float percent = (float)bonus / 100;
        float output = mode switch
        {
            BonusMode.PERCENTMAX => target.Stats.Max[stat] * percent,
            BonusMode.PERCENTCURRENT => target.Stats.Current[stat] * percent,
            BonusMode.PERCENTMISSING => (target.Stats.Max[stat] - target.Stats.Current[stat]) * percent, 
            _ => bonus,
        };
        return Mathf.RoundToInt(output);
    }

    public override IEnumerator PerformEffect(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        ModifyStat(owner, Ability.EffectMagnitude, statType, mode);
        yield break;
    }

    public static void ModifyStat(TinyBot target, int amount, StatType stat, BonusMode mode)
    {
        target.Stats.Current[stat] += GetFinalBonus(amount, mode, target, stat);
    }
}
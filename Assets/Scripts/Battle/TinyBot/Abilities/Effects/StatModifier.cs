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
    public override string Description => GetLineDescription(statType, mode);

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
        foreach(var target in targets)
        {
            TinyBot bot = target as TinyBot;
            if (bot == null) continue;
            ModifyStat(bot, FinalEffectiveness, statType, mode);
        }
        yield break;
    }

    public static void ModifyStat(TinyBot target, int amount, StatType stat, BonusMode mode)
    {
        target.Stats.Current[stat] += GetFinalBonus(amount, mode, target, stat);
    }

    public static string GetLineDescription(StatType stat, BonusMode mode)
    {
        string statWord = stat.ToString().FirstToUpper();
        return mode switch
        {
            BonusMode.PERCENTMAX => $"% of Max {statWord}",
            BonusMode.PERCENTCURRENT => $"% of Current {statWord}",
            BonusMode.PERCENTMISSING => $"% of Missing {statWord}",
            _ => $" {statWord}"
        };
    }
}
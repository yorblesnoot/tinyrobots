using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StatBoost : AbilityEffect
{
    public enum BonusMode
    {
        FLAT,
        PERCENTMAX,
        PERCENTCURRENT
    }


    [SerializeField] StatType statType;
    [SerializeField] BonusMode mode;

    static int GetFinalBonus(int bonus, BonusMode mode, TinyBot target, StatType stat)
    {
        return mode switch
        {
            BonusMode.FLAT => bonus,
            BonusMode.PERCENTMAX => Mathf.RoundToInt(target.Stats.Max[stat] * (float)bonus / 100 + 1),
            BonusMode.PERCENTCURRENT => Mathf.RoundToInt(target.Stats.Current[stat] * (float)bonus / 100 + 1),
            _ => bonus,
        };
    }

    public override IEnumerator PerformEffect(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        ModifyStat(owner, Ability.EffectMagnitude, statType, mode);
        yield break;
    }

    public static void ModifyStat(TinyBot target, int amount, StatType stat, BonusMode mode)
    {
        target.Stats.Current[stat] += GetFinalBonus(amount, mode, target, stat);
        if (target.Allegiance == Allegiance.PLAYER) TurnResourceCounter.Update.Invoke();
    }
}
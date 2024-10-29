using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StatBoost : AbilityEffect
{
    enum BonusMode
    {
        FLAT,
        PERCENTMAX,
        PERCENTCURRENT
    }
    [SerializeField] StatType statType;
    [SerializeField] BonusMode mode;

    int GetFinalBonus(int bonus, BonusMode mode, TinyBot target)
    {
        switch (mode)
        {
            case BonusMode.FLAT:
                return bonus;
            case BonusMode.PERCENTMAX:
                return Mathf.RoundToInt(target.Stats.Max[statType] * (float)bonus / 100 + 1);
            case BonusMode.PERCENTCURRENT:
                return Mathf.RoundToInt(target.Stats.Current[statType] * (float)bonus / 100 + 1);
            default: return bonus;
        }
    }

    public override IEnumerator PerformEffect(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        owner.Stats.Current[statType] += GetFinalBonus(Ability.EffectMagnitude, mode, owner);
        if (owner.Allegiance == Allegiance.PLAYER) TurnResourceCounter.Update.Invoke();
        yield break;
    }
}
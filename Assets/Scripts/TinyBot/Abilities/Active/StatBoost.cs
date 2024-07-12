using UnityEngine;
using System.Collections;

public class StatBoost : ActiveAbility
{
    enum BonusMode
    {
        FLAT,
        PERCENTMAX,
        PERCENTCURRENT
    }
    [SerializeField] StatType statType;
    [SerializeField] BonusMode mode;
    [SerializeField] int bonus;
    protected override IEnumerator PerformEffects()
    {
        Owner.Stats.Current[statType] += GetFinalBonus(bonus, mode);
        if (Owner.Allegiance == Allegiance.PLAYER) TurnResourceCounter.Update.Invoke();
        yield break;
    }

    int GetFinalBonus(int bonus, BonusMode mode)
    {
        switch (mode)
        {
            case BonusMode.FLAT:
                return bonus;
            case BonusMode.PERCENTMAX:
                return Mathf.RoundToInt(Owner.Stats.Max[statType] * (float)bonus / 100 + 1);
            case BonusMode.PERCENTCURRENT:
                return Mathf.RoundToInt(Owner.Stats.Current[statType] * (float)bonus / 100 + 1);
            default: return bonus;
        }
    }
}
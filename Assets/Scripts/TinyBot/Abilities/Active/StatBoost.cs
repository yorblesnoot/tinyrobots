using UnityEngine;
using System.Collections;

public class StatBoost : ActiveAbility
{
    [SerializeField] StatType statType;
    [SerializeField] int bonus;
    protected override IEnumerator PerformEffects()
    {
        Owner.Stats.Current[statType] += 1;
        if (Owner.Allegiance == Allegiance.PLAYER) TurnResourceCounter.Update.Invoke();
        yield break;
    }
}
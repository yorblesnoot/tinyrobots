using System.Collections;
using UnityEngine;

public class IllusionSummon : ActiveAbility
{
    [SerializeField] Material illusionMaterial;
    [SerializeField] BotPalette palette;
    protected override IEnumerator PerformEffects()
    {
        TinyBot summon = Instantiate(Owner);
        summon.transform.position = CurrentTrajectory[^1];
        summon.Initialize();
        summon.Stats.Max[StatType.HEALTH] = 1;
        summon.Stats.MaxAll();
        summon.DamageCalculator.AddFactor(new MultiplierDamage() { Multiplier = (float)EffectMagnitude/100 });
        foreach (var part in summon.PartModifiers) palette.RecolorPart(part, new Material[] { illusionMaterial });
        TurnManager.AddTurnTaker(summon);
        yield break;
    }
}

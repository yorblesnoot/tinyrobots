using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyBuff : AbilityEffect
{
    [SerializeField] bool apply;
    [SerializeField] BuffType buff;
    public override IEnumerator PerformEffect(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        foreach (var target in targets)
        {
            TinyBot bot = target as TinyBot;
            if (bot == null) continue;
            if(apply) bot.Buffs.AddBuff(owner, buff, Ability.EffectMagnitude);
            else bot.Buffs.RemoveBuff(buff);
        }
        yield break;
    }
}

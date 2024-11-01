using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StatBoost;

public class StatBuff : BotBuff
{
    [SerializeField] int amount;
    [SerializeField] StatType statType;
    [SerializeField] BonusMode mode;

    public override void Apply(TinyBot bot)
    {
        ModifyStat(bot, amount, statType, mode);
    }

    public override void Remove(TinyBot bot)
    {
        throw new System.NotImplementedException();
    }
}

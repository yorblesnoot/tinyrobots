using UnityEngine;
using static StatBoost;

[CreateAssetMenu(fileName = "StatBuff", menuName = "ScriptableObjects/Buffs/Stat")]
public class StatBuff : BotBuff
{
    [SerializeField] StatType statType;
    [SerializeField] BonusMode mode;

    public override void ApplyEffect(TinyBot bot, int potency)
    {
        ModifyStat(bot, potency, statType, mode);
    }

    public override void RemoveEffect(TinyBot bot, int potency)
    {
        ModifyStat(bot, -potency, statType, mode);
    }
}

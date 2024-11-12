using UnityEngine;
using static StatModifier;

[CreateAssetMenu(fileName = "StatBuff", menuName = "ScriptableObjects/Buffs/Stat")]
public class StatBuff : BuffType
{
    [SerializeField] StatType statType;
    [SerializeField] BonusMode mode;

    public override void ApplyEffect(TinyBot bot, TinyBot source, int potency)
    {
        ModifyStat(bot, potency, statType, mode);
    }

    public override void RemoveEffect(TinyBot bot, int potency)
    {
        ModifyStat(bot, -potency, statType, mode);
    }
}

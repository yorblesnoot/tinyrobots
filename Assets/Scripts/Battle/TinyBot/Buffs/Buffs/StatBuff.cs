using UnityEngine;
using static StatModifier;

[CreateAssetMenu(fileName = "StatBuff", menuName = "ScriptableObjects/Buffs/Stat")]
public class StatBuff : BuffType
{
    [SerializeField] StatType statType;
    [SerializeField] BonusMode mode;

    public override object ApplyEffect(TinyBot bot, TinyBot source, int potency)
    {
        ModifyStat(bot, potency, statType, mode);
        return null;
    }

    public override void RemoveEffect(TinyBot bot, int potency, object data)
    {
        ModifyStat(bot, -potency, statType, mode);
    }
}

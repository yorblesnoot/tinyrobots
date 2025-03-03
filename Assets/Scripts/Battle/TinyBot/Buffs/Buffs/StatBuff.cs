using UnityEngine;
using static StatModifier;

[CreateAssetMenu(fileName = "StatBuff", menuName = "ScriptableObjects/Buffs/Stat")]
public class StatBuff : BuffType
{
    [SerializeField] StatType statType;
    public override string LineDescription => GetLineDescription(statType, BonusMode.FLAT);

    public override void ApplyEffect(TinyBot bot, TinyBot source, int potency)
    {
        ModifyStat(bot, potency, statType, BonusMode.FLAT);
    }

    public override void RemoveEffect(TinyBot bot, int potency)
    {
        ModifyStat(bot, -potency, statType, BonusMode.FLAT);
    }
}

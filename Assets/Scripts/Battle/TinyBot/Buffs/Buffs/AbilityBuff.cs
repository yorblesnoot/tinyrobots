using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AbilityBuff", menuName = "ScriptableObjects/Buffs/Ability")]
public class AbilityBuff : BuffType
{
    //potency wont work
    [SerializeField] ModType abilityParameter;
    public override void ApplyEffect(TinyBot target, TinyBot source, int potency)
    {
        ModifyAbilities(potency, target);
    }

    public override void RemoveEffect(TinyBot target, int potency)
    {
        ModifyAbilities(-potency, target);
    }

    void ModifyAbilities(int value, TinyBot target)
    {
        KeyValuePair<ModType, int> pair = new(abilityParameter, value);
        foreach(Ability ability in target.Abilities)
        {
            ModdedPart.ApplyMod(ability, pair);
        }
    }

    public override string LineDescription => $" {abilityParameter.ToString().FirstToUpper()}";
}

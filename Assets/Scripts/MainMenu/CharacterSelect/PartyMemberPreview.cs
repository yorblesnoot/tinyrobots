using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class PartyMemberPreview : MonoBehaviour
{
    [SerializeField] TMP_Text charName;
    [SerializeField] TMP_Text energyCapacity;
    [SerializeField] PartStatIcon[] charStats;
    [SerializeField] AbilityDisplay[] abilities;
    [SerializeField] TMP_Text charDesc;
    public void Become(BotCharacter character)
    {
        charName.text = character.GetCoreName();
        energyCapacity.text = character.EnergyCapacity.ToString();
        character.ModdedCore.FinalStats.ToList().PassDataToUI(charStats, (entry, icon) => icon.AssignStat(entry.Key, entry.Value));
        character.ModdedCore.Abilities.PassDataToUI(abilities, (ability, icon) => icon.Become(ability));
        charDesc.text = character.Description;
    }
}

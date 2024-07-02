using TMPro;
using UnityEngine;

public class PartOverviewPanel : MonoBehaviour
{
    [SerializeField] TMP_Text weightDisplay;
    [SerializeField] AbilityDisplay[] abilityDisplays;
    [SerializeField] TMP_Text[] modLines;
    public void Become(ModdedPart part)
    {
        Ability[] activeAbilities = part.Abilities;
        for (int i = 0; i < abilityDisplays.Length; i++)
        {
            bool abilityExists = i < activeAbilities.Length;
            abilityDisplays[i].gameObject.SetActive(abilityExists);
            if (abilityExists) abilityDisplays[i].Become(activeAbilities[i]);
        }
    }
}

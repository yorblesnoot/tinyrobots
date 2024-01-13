using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityUI : MonoBehaviour
{
    [SerializeField] ClickableAbility[] clickableAbilities;
    public void VisualizeAbilityList(List<Ability> abilityList)
    {
        abilityList = abilityList.Where(a => a != null).ToList();
        for (int i = 0; i < abilityList.Count; i++)
        {
            Ability ability = abilityList[i];
            clickableAbilities[i].Become(ability);
        }
    }
}

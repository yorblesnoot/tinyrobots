using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityUI : MonoBehaviour
{
    [SerializeField] ClickableAbility[] clickableAbilities;

    List<ClickableAbility> deployedAbilities;

    readonly static KeyCode[] keyCodes = {KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9 };
    public void VisualizeAbilityList(List<Ability> abilityList)
    {
        deployedAbilities = new();
        abilityList = abilityList.Where(a => a != null).ToList();
        for (int i = 0; i < abilityList.Count; i++)
        {
            Ability ability = abilityList[i];
            clickableAbilities[i].Become(ability, keyCodes[i]);
            deployedAbilities.Add(clickableAbilities[i]);
        }
    }

    private void Update()
    {
        if (deployedAbilities == null || !Input.anyKeyDown) return;
        for (int i = 0;i < deployedAbilities.Count; i++)
        {
            if (Input.GetKeyDown(keyCodes[i]))
            {
                deployedAbilities[i].Activate();
            }
        }
    }
}

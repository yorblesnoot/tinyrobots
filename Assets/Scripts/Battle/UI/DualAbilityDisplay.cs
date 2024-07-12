using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DualAbilityDisplay : AbilityDisplay
{
    [SerializeField] ClickableAbility clickable;
    [SerializeField] UnclickableAbility unclickable;
    public override void Become(Ability ability)
    {
        base.Become(ability);
        clickable.Hide();
        ActiveAbility active = ability as ActiveAbility;
        if(active != null) clickable.Become(active);
        else unclickable.Become(ability as PassiveAbility);
    }
}

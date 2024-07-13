using UnityEngine;

public class UnclickableAbility : AbilityDisplay
{

    public override void Become(Ability ability)
    {
        base.Become(ability);
        PassiveAbility passive = ability as PassiveAbility;
    }
}

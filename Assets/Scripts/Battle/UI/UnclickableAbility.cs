using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnclickableAbility : MonoBehaviour
{
    [HideInInspector] public PassiveAbility Skill;

    public virtual void Become(PassiveAbility ability)
    {
        
    }
}

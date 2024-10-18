using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorPlates : PassiveAbility
{
    bool armored;
    [SerializeField] GameObject armorAura;
    public override void Initialize(TinyBot botUnit)
    {
        base.Initialize(botUnit);
        TurnManager.RoundEnded.AddListener(() => ModifyArmor(true));
        Owner.ReceivedHit.AddListener(() => ModifyArmor(false));
        ModifyArmor(true);
    }

    void ModifyArmor(bool armor)
    {
        if(armor == armored) return;
        armorAura.SetActive(armor);
        armored = armor;
        Owner.Stats.Current[StatType.ARMOR] += armor ? EffectMagnitude : -EffectMagnitude;
    }
}


using UnityEngine;

[CreateAssetMenu(fileName = "ArmorDamage", menuName = "ScriptableObjects/DamageFactors/Armor")]
public class ArmorDamage : DamageFactor
{
    public override float UseFactor(float incoming, TinyBot source, TinyBot target, int potency, TinyBot factorOwner = null)
    {
        float armorMultiplier = 1 - (float)target.Stats.Current[StatType.ARMOR] / 100;
        return armorMultiplier * incoming;
    }
}

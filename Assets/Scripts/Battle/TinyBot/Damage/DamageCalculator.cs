using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DamageCalculator 
{
    List<DamageFactor> factors;

    public DamageCalculator()
    {
        factors = new() { new BackstabDamage(), new ArmorDamage() };
    }
    public void AddFactor(DamageFactor factor)
    {
        factors.Add(factor);
    }

    public void RemoveFactor(DamageFactor factor)
    {
        factors.Remove(factor);
    }

    List<DamageFactor> GetCombinedFactors(TinyBot source)
    {
        List<DamageFactor> finalFactors = new();
        finalFactors.AddRange(factors.Where(f => !f.Outgoing));
        finalFactors.AddRange(source.DamageCalculator.factors.Where(f => f.Outgoing));
        return finalFactors.OrderBy(f => f.Priority).ToList();
    }

    public int GetDamage(int baseDamage, TinyBot source, TinyBot target, bool consume = false)
    {
        List<DamageFactor> finalFactors = GetCombinedFactors(source);
        float currentDamage = baseDamage;
        foreach (var fact in finalFactors)
        {
            currentDamage = fact.UseFactor(currentDamage, source, target);
            if (consume) fact.RemainingUses--;
        }
        return Mathf.RoundToInt(currentDamage);
    }
}

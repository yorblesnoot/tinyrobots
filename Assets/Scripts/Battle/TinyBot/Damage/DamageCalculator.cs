using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DamageCalculator : MonoBehaviour
{
    [SerializeField] List<DamageFactor> baseFactors;
    Dictionary<DamageFactor, AppliedDamageFactor> appliedFactors;
    TinyBot owner;

    private void Awake()
    {
        appliedFactors = baseFactors.ToDictionary(f => f, f => new AppliedDamageFactor(f, 0));
        owner = gameObject.GetComponent<TinyBot>();
    }

    public void AddFactor(AppliedDamageFactor applied)
    {
        appliedFactors.Add(applied.Factor, applied);
    }

    public void RemoveFactor(DamageFactor factor)
    {
        appliedFactors.Remove(factor);
    }

    List<AppliedDamageFactor> GetCombinedFactors(TinyBot source)
    {
        List<AppliedDamageFactor> finalFactors = new();
        finalFactors.AddRange(appliedFactors.Values.Where(f => !f.Factor.Outgoing));
        finalFactors.AddRange(source.DamageCalculator.appliedFactors.Values.Where(f => f.Factor.Outgoing));
        return finalFactors.OrderBy(f => f.Factor.Priority).ToList();
    }

    public int GetDamage(int baseDamage, TinyBot source, TinyBot target, bool consume = false)
    {
        List<AppliedDamageFactor> finalFactors = GetCombinedFactors(source);
        float currentDamage = baseDamage;
        foreach (var fact in finalFactors)
        {
            if (consume) fact.Uses++;
            currentDamage = fact.UseFactor(currentDamage, source, target);
        }
        RemoveExpiredFactors(source.DamageCalculator);
        RemoveExpiredFactors(target.DamageCalculator);
        return Mathf.RoundToInt(currentDamage);
    }

    void RemoveExpiredFactors(DamageCalculator calculator)
    {
        List<AppliedDamageFactor> factors = calculator.appliedFactors.Values.ToList();
        foreach (var applied in factors)
        {
            DamageFactor factor = applied.Factor;
            if(factor.UseLimit > 0 && applied.Uses > factor.UseLimit)
            {
                calculator.owner.Buffs.RemoveBuff(factor);
            }
        }
    }

}

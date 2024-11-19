using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbilityEffect : MonoBehaviour
{
    [HideInInspector] public Ability Ability;
    public int BaseEffectMagnitude = 0;
    public int FinalEffectiveness => Mathf.RoundToInt(Ability == null ? 1 : Ability.EffectivenessMultiplier * BaseEffectMagnitude);
    public virtual string Description => "";
   

    public virtual void Initialize(Ability ability)
    {
        Ability = ability;
    }
    public abstract IEnumerator PerformEffect(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets);
}

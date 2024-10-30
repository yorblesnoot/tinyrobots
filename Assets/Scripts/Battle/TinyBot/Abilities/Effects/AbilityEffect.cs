using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbilityEffect : MonoBehaviour
{
    [HideInInspector] public Ability Ability;
    public virtual void Initialize(Ability ability)
    {
        Ability = ability;
    }
    public abstract IEnumerator PerformEffect(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets);
}

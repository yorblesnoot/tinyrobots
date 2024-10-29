using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbilityEffect : MonoBehaviour
{
    [HideInInspector] public Ability Ability;
    public abstract IEnumerator PerformEffect(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets);
}

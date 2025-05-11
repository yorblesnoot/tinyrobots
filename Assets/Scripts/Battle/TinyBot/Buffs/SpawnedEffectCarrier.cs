using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  Attached to a VFX or other deployed prefab that needs to perform ability effects on spawn and despawn.
/// </summary>
public class SpawnedEffectCarrier : MonoBehaviour
{
    [SerializeField] List<AbilityEffect> activationEffects;
    [SerializeField] List<AbilityEffect> deactivationEffects;

    public void Toggle(bool on, TinyBot owner, TinyBot target)
    {
        StartCoroutine(ToggleSequence(on, owner, target));
    }

    IEnumerator ToggleSequence(bool on, TinyBot owner, TinyBot target)
    {
        List<AbilityEffect> effects = on ? activationEffects : deactivationEffects;
        foreach (var effect in effects) yield return effect.PerformEffect(owner, null, new() { target });
        if(!on) Destroy(gameObject);
    }
}

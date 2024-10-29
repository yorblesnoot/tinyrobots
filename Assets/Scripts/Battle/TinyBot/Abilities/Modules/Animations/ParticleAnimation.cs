using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleAnimation : AbilityEffect
{
    enum ParticleLocation
    {
        BASE,
        DESTINATION
    }
    [SerializeField] ParticleLocation location;
    [SerializeField] ParticleSystem[] particles;

    public override IEnumerator PerformEffect(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        foreach (var particle in particles)
        {
            if (location == ParticleLocation.DESTINATION) particle.gameObject.transform.position = trajectory[^1];
            particle.Play();
        }
        yield break;
    }
}

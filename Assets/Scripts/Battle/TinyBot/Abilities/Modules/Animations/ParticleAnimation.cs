using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleAnimation : AnimationController
{
    enum ParticleLocation
    {
        BASE,
        DESTINATION
    }
    [SerializeField] ParticleLocation location;
    [SerializeField] ParticleSystem[] particles;

    public override IEnumerator Play(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        foreach (var particle in particles)
        {
            if (location == ParticleLocation.DESTINATION) particle.gameObject.transform.position = trajectory[^1];
            particle.Play();
        }
        yield break;
    }
}

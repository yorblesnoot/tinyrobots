using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleAnimation : IKAnimation
{
    [SerializeField] ParticleSystem[] particles;
    public override IEnumerator Play(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        foreach (var particle in particles) particle.Play();
        yield break;
    }
}

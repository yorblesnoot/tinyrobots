using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleAnimation : PartAnimation
{
    [SerializeField] string animatorStateName;
    [SerializeField] ParticleSystem[] particles;
    [SerializeField] Animator animator;
    [SerializeField] bool playValue;

    public override IEnumerator Play(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        animator.SetBool(animatorStateName, playValue);
        foreach (var particle in particles)
        {
            particle.Play();
        }
        yield break;
    }

    public override IEnumerator Stop()
    {
        animator.SetBool(animatorStateName, !playValue);
        yield break;
    }
}

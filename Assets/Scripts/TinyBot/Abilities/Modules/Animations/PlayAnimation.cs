using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAnimation : AnimationController
{
    [SerializeField] string animationName;
    [SerializeField] Animator animator;
    [SerializeField] float duration = .4f;
    public override IEnumerator Play(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        animator.Play(animationName);
        yield return new WaitForSeconds(duration);
    }
}

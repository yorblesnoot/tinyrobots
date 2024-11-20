using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAnimation : AbilityEffect
{
    [SerializeField] string animationName;
    [SerializeField] Animator animator;
    public override IEnumerator PerformEffect(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        animator.Play(animationName);
        float duration = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(duration);
    }
}

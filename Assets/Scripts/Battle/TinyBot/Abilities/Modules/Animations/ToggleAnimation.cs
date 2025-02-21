using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleAnimation : AbilityEffect
{
    [SerializeField] string animatorStateName;
    [SerializeField] Animator animator;
    [SerializeField] float duration = 0;
    [SerializeField] bool playValue;

    public override IEnumerator PerformEffect(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        animator.speed = 1;
        animator.SetBool(animatorStateName, playValue);
        yield return new WaitForSeconds(duration);
    }

    public void Stop()
    {
        animator.SetBool(animatorStateName, !playValue);
    }
}

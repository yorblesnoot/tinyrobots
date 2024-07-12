using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleAnimation : ProceduralAnimation
{
    [SerializeField] string animatorStateName;
    [SerializeField] Animator animator;
    [SerializeField] bool playValue;

    public override IEnumerator Play(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        animator.SetBool(animatorStateName, playValue);
        yield break;
    }

    public void Stop()
    {
        animator.SetBool(animatorStateName, !playValue);
    }
}

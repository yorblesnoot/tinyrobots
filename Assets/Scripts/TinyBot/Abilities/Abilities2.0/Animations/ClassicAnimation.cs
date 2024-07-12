using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassicAnimation : ProceduralAnimation
{
    [SerializeField] string animationName;
    [SerializeField] Animator animator;
    public override IEnumerator Play(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        animator.Play(animationName);
        yield return new WaitForSeconds(.4f);
    }
}

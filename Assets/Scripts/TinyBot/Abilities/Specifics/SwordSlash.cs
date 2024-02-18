using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordSlash : ConicAbility
{
    [SerializeField] float coneDiameter = 1f;
    [SerializeField] float coneLength = 1f;
    [SerializeField] GameObject aimCone;
    [SerializeField] Animator animator;
    private void Start()
    {
        aimCone.transform.localScale = new(coneDiameter, coneLength, coneDiameter);
    }
    public override GameObject GhostAimAt(GameObject target, Vector3 sourcePosition)
    {
        throw new System.NotImplementedException();
    }

    protected override void AimAt(GameObject target)
    {
        aimCone.transform.LookAt(target.transform);
    }

    public override void LockOnTo(GameObject target)
    {
        base.LockOnTo(target);
        animator.SetBool("bladeOut", true);
    }

    protected override IEnumerator PerformEffects()
    {
        throw new System.NotImplementedException();
    }
}

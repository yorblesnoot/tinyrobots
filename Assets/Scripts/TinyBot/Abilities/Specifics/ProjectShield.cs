using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectShield : LinearAbility
{
    [SerializeField] GameObject ikTarget;
    [SerializeField] float shieldDistance = 1f;
    [SerializeField] string aniBool = "shieldUp";
    [SerializeField] float slowness = 100f;
    [SerializeField] Animator animator;
    public override IEnumerator ExecuteAbility(Vector3 target)
    {
        animator.SetBool("barrierUp", true);
        yield break;
    }

    private void Start()
    {
        //ikTarget.transform.position -= owner.transform.up;
    }

    protected override void AimAt(GameObject target)
    {
        base.AimAt(target);
        Vector3 ownerPosition = owner.ChassisPoint.transform.position;
        Vector3 direction = target.transform.position - ownerPosition;
        direction.Normalize();
        direction *= shieldDistance;
        Vector3 finalPosition = ownerPosition + direction;
        ikTarget.transform.position = Vector3.Lerp(ikTarget.transform.position, finalPosition, 1 / slowness);
    }

    public override void LockOnTo(GameObject target)
    {
        base.LockOnTo(target);
    }
}

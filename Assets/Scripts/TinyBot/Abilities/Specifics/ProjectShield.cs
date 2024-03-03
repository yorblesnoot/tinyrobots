using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectShield : LinearAbility
{
    [SerializeField] GameObject ikTarget;
    [SerializeField] float shieldDistance = 1f;
    [SerializeField] float slowness = 50f;
    [SerializeField] Animator animator;
    Vector3 basePosition;
    protected override IEnumerator PerformEffects()
    {
        animator.SetBool("barrierUp", true);
        owner.beganTurn.AddListener(BeginLowerShield);
        yield break;
    }

    void BeginLowerShield()
    {
        owner.beganTurn.RemoveListener(BeginLowerShield);
        StartCoroutine(LowerShield());
    }

    IEnumerator LowerShield()
    {
        animator.SetBool("barrierUp", false);
        yield return new WaitForSeconds(1f);
        StartCoroutine(ikTarget.LerpTo(basePosition, 1f, true));
    }

    private void Start()
    {
        basePosition = ikTarget.transform.localPosition;
    }

    protected override List<TinyBot> AimAt(GameObject target, Vector3 sourcePosition, bool drawLine)
    {
        base.AimAt(target, sourcePosition, true);
        Vector3 ownerPosition = transform.position;
        Vector3 direction = target.transform.position - ownerPosition;
        direction.Normalize();
        direction *= shieldDistance;
        Vector3 finalPosition = ownerPosition + direction;
        ikTarget.transform.position = Vector3.Lerp(ikTarget.transform.position, finalPosition, 1 / slowness);
        return new();
    }

    public override void LockOnTo(GameObject target)
    {
        base.LockOnTo(target);
    }
}

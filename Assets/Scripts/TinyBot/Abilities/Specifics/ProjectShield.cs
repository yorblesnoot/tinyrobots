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
        Owner.BeganTurn.AddListener(NeutralAim);
        yield break;
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

    public override List<TinyBot> AimAt(GameObject target, Vector3 sourcePosition, bool aiMode = false)
    {
        base.AimAt(target, sourcePosition);
        Vector3 ownerPosition = transform.position;
        Vector3 direction = target.transform.position - ownerPosition;
        direction.Normalize();
        direction *= shieldDistance;
        Vector3 finalPosition = ownerPosition + direction;
        ikTarget.transform.position = Vector3.Lerp(ikTarget.transform.position, finalPosition, 1 / slowness);
        return new();
    }

    public override void LockOnTo(GameObject target, bool draw)
    {
        base.LockOnTo(target, true);
    }

    public override void NeutralAim()
    {
        Owner.BeganTurn.RemoveListener(NeutralAim);
        StartCoroutine(LowerShield());
    }
}

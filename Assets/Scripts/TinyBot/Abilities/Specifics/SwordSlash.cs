using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SwordSlash : SpatialAbility
{
    [SerializeField] GameObject aimer;
    [SerializeField] SpatialTargeter indicator;
    [SerializeField] Animator animator;
    [SerializeField] Transform ikTarget;
    [SerializeField] Transform readyPosition;
    [SerializeField] Transform slashPosition;

    [SerializeField] float slashHeight;
    [SerializeField] float slashWidth;
    [SerializeField] float slashLength;

    [SerializeField] float slashTime = .2f;
    [SerializeField] float returnTime = .3f;

    [SerializeField] float aimLag = 30;


    Vector3 neutralPosition;
    private void Start()
    {
        neutralPosition = ikTarget.transform.localPosition;
    }

    public override List<TinyBot> AimAt(GameObject target, Vector3 sourcePosition, bool aiMode = false)
    {
        if (aiMode)
        {
            List<TinyBot> targets = new();
            Collider[] colliders = Physics.OverlapSphere(sourcePosition, range, LayerMask.GetMask("Default"));
            foreach (Collider collider in colliders)
            {
                if(collider.TryGetComponent(out TinyBot bot)) targets.Add(bot);
            }
            return targets;
        }
        else
        {
            aimer.transform.LookAt(target.transform);
            ikTarget.position = Vector3.Lerp(ikTarget.position, readyPosition.position, 1 / aimLag);
            return indicator.GetIntersectingBots();
        }
        
    }

    public override void LockOnTo(GameObject target, bool draw)
    {
        base.LockOnTo(target, draw);
        animator.SetBool("bladeOut", true);
        indicator.gameObject.SetActive(true);
        if (!draw) indicator.GetComponent<Renderer>().enabled = false;
    }

    public override void ReleaseLockOn()
    {
        base.ReleaseLockOn();
        indicator.gameObject.SetActive(false);
    }

    protected override IEnumerator PerformEffects()
    {
        List<TinyBot> hitTargets = indicator.GetIntersectingBots().Where(bot => bot != Owner).ToList();
        indicator.ResetIntersecting();
        ReleaseLockOn();
        Vector3[] slashPoints = GetSlashPoints();

        Vector3 thrustTarget = (slashPoints[0] + slashPoints[1])/2;
        //yield return StartCoroutine(Owner.PrimaryMovement.ApplyImpulseToBody(thrustTarget, -.5f, returnTime, slashTime/2));
        yield return StartCoroutine(ikTarget.gameObject.LerpTo(slashPoints[0], slashTime));
        StartCoroutine(Owner.PrimaryMovement.ApplyImpulseToBody(thrustTarget, -1, slashTime, returnTime * 2));
        yield return StartCoroutine(SlashThroughPoints(slashPoints, slashTime));
        foreach (TinyBot bot in hitTargets)
        {
            bot.ReceiveDamage(damage, Owner.transform.position, bot.ChassisPoint.position);
        }
        yield return new WaitForSeconds(returnTime);
        yield return StartCoroutine(ikTarget.gameObject.LerpTo(neutralPosition, returnTime, true));
        animator.SetBool("bladeOut", false);
    }

    IEnumerator SlashThroughPoints(Vector3[] points, float duration)
    {
        float timeElapsed = 0;
        Vector3 startPosition = Owner.transform.InverseTransformPoint(points[0]);
        Vector3 endPosition = Owner.transform.InverseTransformPoint(points[1]);
        while (timeElapsed < duration)
        {
            Vector3 localPosition = Vector3.Slerp(startPosition, endPosition, timeElapsed / duration);
            ikTarget.transform.position = Owner.transform.TransformPoint(localPosition);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        
    }

    Vector3[] GetSlashPoints()
    {
        Vector3 awayFromBody = (slashPosition.position - Owner.ChassisPoint.position).normalized;
        Vector3 startPosition = slashPosition.position + Owner.transform.up * slashHeight + awayFromBody * slashWidth;

        Vector3 lastPosition = Vector3.Reflect(-Owner.ChassisPoint.InverseTransformPoint(startPosition), Vector3.forward);
        lastPosition = Owner.ChassisPoint.TransformPoint(lastPosition);
        return new Vector3[] { startPosition, lastPosition };

    }

    public override void NeutralAim()
    {
        StartCoroutine(ikTarget.gameObject.LerpTo(neutralPosition, .5f, true));
    }
}

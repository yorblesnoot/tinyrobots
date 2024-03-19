using System.Collections;
using System.Collections.Generic;
using System.Drawing;
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

    float indicatorRange;

    Vector3 neutralPosition;
    private void Start()
    {
        neutralPosition = ikTarget.transform.localPosition;
        indicatorRange = indicator.GetComponent<Renderer>().bounds.size.magnitude/2;
    }

    public override List<TinyBot> AimAt(GameObject target, Vector3 sourcePosition, bool aiMode = false)
    {
        if (aiMode)
        {
            List<TinyBot> targets = new();
            if (Vector3.Distance(target.transform.position, sourcePosition) <= indicatorRange && target.TryGetComponent(out TinyBot bot))
                targets.Add(bot);
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
        if (!draw) return;
        indicator.gameObject.SetActive(true);
        animator.SetBool("bladeOut", true);
    }

    public override void ReleaseLock()
    {
        base.ReleaseLock();
        indicator.gameObject.SetActive(false);
        StartCoroutine(ikTarget.gameObject.LerpTo(neutralPosition, .5f, true));
    }

    protected override IEnumerator PerformEffects()
    {
        List<TinyBot> hitTargets = indicator.GetIntersectingBots().Where(bot => bot != Owner).ToList();
        indicator.ResetIntersecting();
        ReleaseLock();
        Vector3[] slashPoints = GetSlashPoints();
        
        yield return StartCoroutine(ikTarget.gameObject.LerpTo(slashPoints[0], returnTime));
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
            Owner.PrimaryMovement.RotateToTrackEntity(ikTarget.gameObject);
            Vector3 localPosition = Vector3.Slerp(startPosition, endPosition, timeElapsed / duration);
            ikTarget.transform.position = Owner.transform.TransformPoint(localPosition);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        
    }

    Vector3[] GetSlashPoints()
    {
        Vector3 awayFromBody = (transform.position - Owner.ChassisPoint.position).normalized;
        Vector3 startPosition = slashPosition.position + Vector3.up * slashHeight + awayFromBody * slashWidth;
        Vector3 direction = (slashPosition.position - startPosition).normalized;
        Vector3 lastPosition = startPosition + direction * slashLength;
        return new Vector3[] { startPosition, lastPosition };

    }
}

using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArmGrab : SpatialAbility
{
    [SerializeField] ArmThrow armThrow;
    [SerializeField] float carryHeight = 1;
    [SerializeField] float armMoveDuration = .5f;
    [SerializeField] float grabPointDistance = .5f;
    public override List<TinyBot> AimAt(GameObject target, Vector3 sourcePosition, bool aiMode = false)
    {
        Vector3 rangeLineOrigin = transform.position;
        Vector3 direction = target.transform.position - rangeLineOrigin;
        float distance = Vector3.Distance(target.transform.position, rangeLineOrigin);
        direction.Normalize();
        Vector3 endPoint = rangeLineOrigin + direction * Mathf.Min(range, distance);
        Vector3 targetPosition = Vector3.Lerp(ikTarget.position, endPoint, Time.deltaTime);
        ikTarget.transform.position = targetPosition;
        indicator.transform.position = targetPosition;
        return indicator.GetIntersectingBots().Take(1).ToList();
    }

    public override void LockOnTo(GameObject target, bool draw)
    {
        base.LockOnTo(target, draw);
        indicator.gameObject.SetActive(true);
    }

    protected override IEnumerator PerformEffects()
    {
        TinyBot target = indicator.GetIntersectingBots()[0];
        indicator.gameObject.SetActive(false);
        target.ToggleActiveLayer(true);
        yield return Tween.Position(ikTarget, endValue: target.ChassisPoint.position, duration: armMoveDuration).ToYieldInstruction();
        target.transform.SetParent(emissionPoint.transform, true);
        Vector3 holdPosition = Owner.transform.up * carryHeight;
        holdPosition += transform.position + Owner.transform.forward;
        Tween.Position(ikTarget, endValue: holdPosition, duration: armMoveDuration);
        armThrow.PrepareToThrow(target);
    }

    

    public override bool IsUsable(Vector3 targetPosition)
    {
        if(!base.IsUsable(targetPosition)) return false;
        if(indicator.GetIntersectingBots().Count == 0) return false;
        return true;
    }
}

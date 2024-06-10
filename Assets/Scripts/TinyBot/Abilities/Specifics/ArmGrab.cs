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
    public override List<TinyBot> AimAt(GameObject target, Vector3 sourcePosition, bool aiMode = false)
    {
        Vector3 direction = target.transform.position - Owner.transform.position;
        direction.Normalize();
        Vector3 endPoint = Owner.transform.position + direction * range;
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
        yield return new WaitForSeconds(armMoveDuration);
        target.transform.SetParent(emissionPoint.transform, true);
        Vector3 holdPosition = new(0, carryHeight, 0);
        Tween.LocalPosition(ikTarget, endValue: holdPosition, duration: armMoveDuration);
    }

    public override bool IsUsable(Vector3 targetPosition)
    {
        if(!base.IsUsable(targetPosition)) return false;
        if(indicator.GetIntersectingBots().Count == 0) return false;
        return true;
    }
}

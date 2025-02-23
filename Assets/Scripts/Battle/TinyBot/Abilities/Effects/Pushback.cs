using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pushback : AbilityEffect
{
    [SerializeField] float pushScanRadius = .5f;
    [SerializeField] float pushDuration = .5f;
    int layerMask;
    private void Awake()
    {
        layerMask = LayerMask.GetMask("Default", "Terrain");
    }
    public override IEnumerator PerformEffect(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        Vector3 pushDirection = trajectory[^1] - trajectory[^2];
        pushDirection.Normalize();
        Sequence tween = Sequence.Create();
        foreach (Targetable target in targets)
        {
            bool hitSomething = Physics.SphereCast(target.TargetPoint.position, pushScanRadius, pushDirection, out RaycastHit hit, FinalEffectiveness, layerMask);
            Vector3 destination = /*hitSomething ? hit.point :*/ target.transform.position + pushDirection * FinalEffectiveness;
            //tween.Group(Tween.Position(target.transform, destination, duration: pushDuration));
            Tween.Position(target.transform, destination, duration: pushDuration);
            Debug.DrawLine(destination, destination + Vector3.up * 3, Color.white, 20);
        }
        //yield return tween.ToYieldInstruction();
        //foreach (Targetable target in targets) StartCoroutine(target.Fall());
        yield break;
    }


}

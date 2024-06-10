using PrimeTween;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpatialAbility : Ability
{
    [SerializeField] protected Transform ikTarget;
    [SerializeField] protected SpatialTargeter indicator;
    [SerializeField] protected Animator animator;

    protected Vector3 neutralPosition;
    private void Start()
    {
        neutralPosition = ikTarget.transform.localPosition;
    }

    public override void NeutralAim()
    {
        Tween.LocalPosition(ikTarget, endValue: neutralPosition, duration: .5f);
    }
}

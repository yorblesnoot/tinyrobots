using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TrackingAnimation : MonoBehaviour
{
    [SerializeField] protected Transform ikTarget;

    Vector3 basePosition;
    void Awake()
    {
        basePosition = ikTarget.localPosition;
    }

    public abstract void Aim(List<Vector3> trajectory);

    public virtual void ResetTracking()
    {
        Tween.LocalPosition(ikTarget, endValue: basePosition, duration: .5f);
    }
}

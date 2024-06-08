using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TrackingAnimation : MonoBehaviour
{
    [SerializeField] protected Transform aimTarget;

    Vector3 basePosition;
    void Awake()
    {
        basePosition = aimTarget.localPosition;
    }

    public abstract void TrackTrajectory(List<Vector3> trajectory);

    public void ResetTracking()
    {
        Tween.LocalPosition(aimTarget, endValue: basePosition, duration: .5f);
    }
}

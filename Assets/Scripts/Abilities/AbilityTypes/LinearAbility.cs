using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LinearAbility : Ability
{
    public override void ControlTargetLine()
    {
        Vector3 direction = (PrimaryCursor.Transform.position - emissionPoint.transform.position).normalized;
        Vector3 endPoint = emissionPoint.transform.position + direction * maxRange;
        LineMaker.DrawLine(emissionPoint.transform.position, endPoint);
    }
}

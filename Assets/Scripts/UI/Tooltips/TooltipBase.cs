using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TooltipBase : MonoBehaviour
{
    [SerializeField] protected float heightModifier = 50;
    [SerializeField] protected bool usePosition;
    protected void SetPosition(Vector3 position)
    {
        if (usePosition) return;
        position = transform.parent.InverseTransformPoint(position);
        float distance = position.magnitude;
        distance -= heightModifier;
        position.Normalize();
        Vector3 finalPosition = position * distance;
        transform.localPosition = finalPosition;
    }


}

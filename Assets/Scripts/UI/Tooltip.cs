using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    [SerializeField] protected float heightModifier = 50;
    [SerializeField] protected bool usePosition;
    Image image;
    protected void SetPosition(Vector3 position)
    {
        if (usePosition) return;
        if(image == null) image = GetComponent<Image>();
        float showHeight = image.rectTransform.rect.height / 2 + heightModifier;
        position = transform.parent.InverseTransformPoint(position);
        position.y += position.y > 0 ? -showHeight : showHeight;
        transform.localPosition = position;
    }


}

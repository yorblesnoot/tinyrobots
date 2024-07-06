using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static class UIHelper
{
    public static void PassDataToUI<D, E>(this IReadOnlyList<D> data, IReadOnlyList<E> elements, UnityAction<D, E> dataFound) where E : MonoBehaviour
    {
        for(int i = 0; i < elements.Count; i++)
        {
            bool hasData = i < data.Count;
            elements[i].gameObject.SetActive(hasData);
            if (hasData) dataFound(data[i], elements[i]);
        }
    }

    public static Vector3 WorldToCanvasPosition(this Vector3 position, Canvas canvas, Camera camera)
    {
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(camera, position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : camera, out Vector2 result);

        return canvas.transform.TransformPoint(result);
    }
}

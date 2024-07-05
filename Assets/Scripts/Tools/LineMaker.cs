using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineMaker : MonoBehaviour
{
    [SerializeField] LineRenderer lineRenderer;
    static LineRenderer LineRenderer;
    private void Awake()
    {
        LineRenderer = lineRenderer;
    }

    public static void DrawLine(params Vector3[] locations)
    {
        LineRenderer.positionCount = locations.Length;
        LineRenderer.SetPositions(locations);
    }

    public static void HideLine()
    {
        LineRenderer.positionCount = 0;
    }


}

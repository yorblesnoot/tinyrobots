using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DanglingRope : MonoBehaviour
{
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] Transform sourcePoint;
    [SerializeField] Transform endPoint;
    [SerializeField] Transform handle;
    [SerializeField] Transform spring;
    [SerializeField] float ropeLength = 5;
    [SerializeField] int pointCount = 10;

    private void Awake()
    {
        lineRenderer.useWorldSpace = true;
        Transform[] transforms = new Transform[] { sourcePoint, endPoint, handle, spring };
        foreach (Transform t in transforms) t.GetComponent<MeshRenderer>().enabled = false;
    }

    private void Update()
    {
        Vector3 handle = GetHandlePosition(sourcePoint.position, endPoint.position);
        Vector3[] points = new Vector3[pointCount + 1];
        for(int i = 0; i <= pointCount; i++)
        {
            float progress = (float)i/pointCount;
            Vector3 a = Vector3.Lerp(sourcePoint.position, handle, progress);
            Vector3 b = Vector3.Lerp(handle, endPoint.position, progress);
            Vector3 point = Vector3.Lerp(a, b, progress);
            points[i] = point;
        }
        lineRenderer.positionCount = pointCount + 1;
        lineRenderer.SetPositions(points);
    }

    Vector3 GetHandlePosition(Vector3 source, Vector3 end)
    {
        float distance = Vector3.Distance(source, end);
        distance = Mathf.Clamp(distance, 0, ropeLength);
        float slack = Mathf.Lerp(ropeLength / 2, 0, distance / ropeLength);
        Vector3 targetPoint = Vector3.Lerp(source, end, .5f);
        targetPoint.y -= slack;
        handle.position = targetPoint;
        return spring.position;
    }
}

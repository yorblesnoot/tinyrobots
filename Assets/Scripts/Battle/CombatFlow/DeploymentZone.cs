using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeploymentZone : MonoBehaviour
{
    [SerializeField] GameObject zoneMarker;
    [SerializeField] Transform corner;
    static DeploymentZone active;
    private void Awake()
    {
        active = this;
        corner.gameObject.SetActive(false);
        zoneMarker.SetActive(false);
    }

    public static void BeginDeployment()
    {
        active.zoneMarker.SetActive(true);
        active.PositionAreaMarker();
        MainCameraControl.FindViewOfPosition(active.zoneMarker.transform.position);
    }

    public static void EndDeployment()
    {
        active.zoneMarker.SetActive(false);
    }

    void PositionAreaMarker()
    {
        Vector3 scale = corner.localPosition / 2;
        scale.y = 1;
        zoneMarker.transform.localScale = scale;

        Vector3 position = corner.localPosition / 2;
        zoneMarker.transform.localPosition = position;
    }
    

    public static Vector3 ClampInZone(Vector3 position)
    {
        Vector3 localPosition = active.transform.InverseTransformPoint(position);
        localPosition = localPosition.Clamp(Vector3.zero, active.corner.localPosition);
        Vector3 finalPosition = active.transform.TransformPoint(localPosition);
        return finalPosition;
    }

    private void OnDrawGizmos()
    {
        if (corner == null) return;
        PositionAreaMarker();
    }
}

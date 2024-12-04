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
        Vector3 scale = (transform.position - corner.position) / 2;
        scale.y = 1;
        zoneMarker.transform.localScale = scale;

        Vector3 position = (transform.position + corner.position) / 2;
        zoneMarker.transform.position = position;
    }
    

    public static Vector3 ClampInZone(Vector3 position)
    {
        return position.Clamp(active.transform.position, active.corner.position);
    }

    private void OnDrawGizmos()
    {
        if (corner == null) return;
        PositionAreaMarker();
        GizmoPlus.DrawWireCuboid(transform.position, corner.position, Color.green);
    }
}

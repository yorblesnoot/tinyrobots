using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeploymentZone : MonoBehaviour
{
    [SerializeField] GameObject zoneMarker;
    [SerializeField] Transform corner;
    static DeploymentZone Active;
    private void Awake()
    {
        Active = this;
        corner.gameObject.SetActive(false);
        zoneMarker.SetActive(false);
    }

    public static void BeginDeployment()
    {
        Active.zoneMarker.SetActive(true);
        Active.PositionAreaMarker();
        MainCameraControl.CutToEntity(Active.zoneMarker.transform);
    }

    public static void EndDeployment()
    {
        Active.zoneMarker.SetActive(false);
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
        return position.Clamp(Active.transform.position, Active.corner.position);
    }

    private void OnDrawGizmos()
    {
        if (corner == null) return;
        PositionAreaMarker();
        GizmoPlus.DrawWireCuboid(transform.position, corner.position, Color.green);
    }
}

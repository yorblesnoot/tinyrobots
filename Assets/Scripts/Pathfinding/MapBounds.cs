using System.Collections.Generic;
using UnityEngine;

public class MapBounds : MonoBehaviour
{
    [SerializeField] Transform outerCorner;
    public Transform MapContainer;
    private void Awake()
    {
        gameObject.GetComponent<Renderer>().enabled = false;
        outerCorner.GetComponent<Renderer>().enabled = false;
    }

    public Vector3Int GetMapSize()
    {
        return Vector3Int.FloorToInt(outerCorner.position);
    }

    private void OnDrawGizmos()
    {
        if (outerCorner == null) return;
        GizmoPlus.DrawWireCuboid(transform.position, outerCorner.transform.position, Color.blue);
    }
}

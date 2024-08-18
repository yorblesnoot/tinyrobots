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
        if(outerCorner == null) return;
        Gizmos.color = Color.blue;
        Vector3[] points = new Vector3[6];
        List<Vector3> lines = new();
        for (int i = 0; i < 3; i++)
        {
            points[i] = transform.position;
            points[i][i] = outerCorner.transform.position[i];
            lines.Add(transform.position);
            lines.Add(points[i]);
        }
        for (int i = 0; i < 3; i++)
        {
            int ip = i + 3;
            points[ip] = outerCorner.transform.position;
            points[ip][i] = transform.position[i];
            lines.Add(outerCorner.transform.position);
            lines.Add(points[ip]);
        }
        lines.Add(points[0]); lines.Add(points[5]);
        lines.Add(points[0]); lines.Add(points[4]);
        lines.Add(points[1]); lines.Add(points[5]);
        lines.Add(points[1]); lines.Add(points[3]);
        lines.Add(points[2]); lines.Add(points[3]);
        lines.Add(points[2]); lines.Add(points[4]);
        Gizmos.DrawLineList(lines.ToArray());
    }
}

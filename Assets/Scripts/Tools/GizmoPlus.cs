using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GizmoPlus
{
    public static void DrawWireCuboid(Vector3 innerCorner, Vector3 outerCorner, Color color)
    {
        Gizmos.color = color;
        Vector3[] points = new Vector3[6];
        List<Vector3> lines = new();
        for (int i = 0; i < 3; i++)
        {
            points[i] = innerCorner;
            points[i][i] = outerCorner[i];
            lines.Add(innerCorner);
            lines.Add(points[i]);
        }
        for (int i = 0; i < 3; i++)
        {
            int ip = i + 3;
            points[ip] = outerCorner;
            points[ip][i] = innerCorner[i];
            lines.Add(outerCorner);
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

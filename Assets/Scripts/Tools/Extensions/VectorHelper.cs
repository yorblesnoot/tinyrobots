using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class VectorHelper
{
    public static Vector3 FastNormalize(this Vector3 input)
    {
        return input / Mathf.Sqrt(input.sqrMagnitude);
    }

    public static Vector3 Clamp(this Vector3 value, Vector3 min, Vector3 max)
    {
        value.x = Mathf.Clamp(value.x, min.x, max.x);
        value.y = Mathf.Clamp(value.y, min.y, max.y);
        value.z = Mathf.Clamp(value.z, min.z, max.z);
        return value;
    }

    public static Vector3 Average(this IEnumerable<Vector3> vectors)
    {
        Vector3 total = Vector3.zero;
        foreach (var vector in vectors)
        {
            total += vector;
        }
        return total / vectors.Count();
    }

    public static float GetTotalDistance(this IEnumerable<Vector3> vectors)
    {
        float totalDistance = 0f;
        Vector3 lastVector = Vector3.zero;
        bool first = true;
        foreach (var vector in vectors)
        {
            if (first)
            {
                lastVector = vector;
                first = false;
                continue;
            }
            totalDistance += Vector3.Distance(lastVector, vector);
            lastVector = vector;
        }
        return totalDistance;
    }
}

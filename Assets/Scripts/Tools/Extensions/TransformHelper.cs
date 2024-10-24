using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformHelper 
{
    public static void TraverseHierarchy(this Transform transform, Action<Transform> action)
    {
        action(transform);
        foreach(Transform child in transform)
        {
            child.TraverseHierarchy(action);
        }
    }
}

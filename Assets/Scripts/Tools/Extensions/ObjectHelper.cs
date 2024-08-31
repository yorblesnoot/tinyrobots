using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ObjectHelper 
{
    public static T AddComponentIfNeeded<T>(this Component obj) where T : Component
    {
        if (!obj.TryGetComponent<T>(out var component)) component = obj.gameObject.AddComponent<T>();
        return component;
    }
}

using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapBounds))]
public class BoundsEditor : Editor
{
    int terrainLayer;
    private void Awake()
    {
        terrainLayer = LayerMask.NameToLayer("Terrain");
    }
    public override void OnInspectorGUI()
    {
        MapBounds bounds = target as MapBounds;
        if (GUILayout.Button("Configure Map Objects"))
        {
            TraverseHierarchy(bounds.MapContainer, ConfigureTerrain);
        }
        DrawDefaultInspector();
    }

    void TraverseHierarchy(Transform parent, Action<Transform> command)
    {
        foreach(Transform child in parent)
        {
            command(child);
            TraverseHierarchy(child, command);
        }
    }

    void ConfigureTerrain(Transform child)
    {
        child.gameObject.layer = terrainLayer;
        if(!child.gameObject.TryGetComponent<MeshCollider>(out _)) child.gameObject.AddComponent<MeshCollider>();
        EditorUtility.SetDirty(child.gameObject);
    }
}
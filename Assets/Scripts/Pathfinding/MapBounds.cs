using System;
using UnityEditor;
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
        return Vector3Int.CeilToInt(outerCorner.position);
    }

    private void OnDrawGizmos()
    {
        if (outerCorner == null) return;
        GizmoPlus.DrawWireCuboid(transform.position, outerCorner.transform.position, Color.blue);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MapBounds))]
public class BoundsEditor : Editor
{
    int terrainLayer;
    int interiorLayer;
    private void Awake()
    {
        terrainLayer = LayerMask.NameToLayer("Terrain");
        interiorLayer = LayerMask.NameToLayer("TerrainInterior");
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
        foreach (Transform child in parent)
        {
            command(child);
            TraverseHierarchy(child, command);
        }
    }

    void ConfigureTerrain(Transform child)
    {
        if (child.GetComponent<MeshRenderer>() == null) return;
        child.gameObject.layer = child.gameObject.layer == interiorLayer ? interiorLayer : terrainLayer;
        if(child.gameObject.TryGetComponent(out Collider collider)) DestroyImmediate(collider);
        child.gameObject.AddComponent<MeshCollider>();
        EditorUtility.SetDirty(child.gameObject);
    }
}
#endif

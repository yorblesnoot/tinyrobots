using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SliceTargeter))]
public class SliceEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SliceTargeter slicer = (SliceTargeter)target;
        if (GUILayout.Button("Configure Slices"))
        {
            slicer.Slices = new();
            foreach (Transform slice in slicer.transform)
            {
                MeshCollider collider = slice.AddComponentIfNeeded<MeshCollider>();
                collider.convex = true;
                collider.isTrigger = true;
                slice.gameObject.layer = slicer.SensorLayer;
                slicer.Slices.Add(slice.GetComponent<MeshRenderer>());
                EditorUtility.SetDirty(slice.gameObject);
            }
        }
        DrawDefaultInspector();
    }
}

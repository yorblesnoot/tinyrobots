using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SliceTargeter : MonoBehaviour
{
    public int SensorLayer = 11;
    public List<MeshRenderer> Slices;
    [SerializeField] SpatialSensor sensor;
    static SliceTargeter instance;
    public static Transform Transform;
    public static SpatialSensor Sensor;
    static int active = 0;
    private void Awake()
    {
        instance = this;
        Transform = transform;
        Sensor = sensor;
    }

    public static void SetShape(int degree, float radius)
    {
        instance.gameObject.SetActive(true);
        int count = instance.Slices.Count;
        float mod = 1 - (float)degree / 360;
        mod *= count;
        int finalIndex = Mathf.FloorToInt(mod);
        active = finalIndex;
        Transform.localScale = new Vector3(radius, radius, radius);
        for (int i = 0; i < count; i++)
        {
            MeshRenderer renderer = instance.Slices[i];
            renderer.gameObject.SetActive(i == finalIndex);
            renderer.enabled = i == finalIndex;
        }
    }

    public static void Hide()
    {
        instance.gameObject.SetActive(false);
    }

    public static void ToggleVisual(bool setting = true)
    {
        instance.Slices[active].enabled = setting;
    }
}

#if UNITY_EDITOR
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
#endif

#if UNITY_EDITOR
using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class CameraAligner : MonoBehaviour
{
    [SerializeField] int height;
    [SerializeField] int horizontal;

    [SerializeField] List<CinemachineVirtualCamera> angles;
    [SerializeField] CinemachineFreeLook freeCam;

    readonly Vector3[] directions = { new(1, 1, 1), new(-1, 1, 1), new(1, 1, -1), new(-1, 1, -1),
                                      new(1, -1, 1), new(-1, -1, 1), new(1, -1, -1), new(-1, -1, -1)};

    public void SetCameras()
    {
        List<Vector3> finalPositions = directions
            .Select(direction => new Vector3(direction.x * horizontal, direction.y * height, direction.z * horizontal))
            .ToList();
        for (int i = 0; i < finalPositions.Count; ++i)
        {
            CinemachineTransposer transposer = angles[i].GetCinemachineComponent<CinemachineTransposer>();
            transposer.m_FollowOffset = finalPositions[i];
            EditorUtility.SetDirty(transposer);
        }
        SetFreeCamera(finalPositions[0].magnitude);
    }

    void SetFreeCamera(float radius)
    {
        float secondaryRadius = new Vector2(horizontal, horizontal).magnitude;
        ConfigureRig(0, secondaryRadius, height);
        ConfigureRig(1, radius, 0);
        ConfigureRig(2, secondaryRadius, -height);
        EditorUtility.SetDirty(freeCam);
    }

    void ConfigureRig(int index, float radius, float height)
    {
        freeCam.m_Orbits[index].m_Radius = radius;
        freeCam.m_Orbits[index].m_Height = height;
    }
}


[CustomEditor(typeof(CameraAligner))]


public class CameraAlignerEditor : Editor
{
    
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Configure Cameras"))
        {
            CameraAligner aligner = target as CameraAligner;
            aligner.SetCameras();
        }
        DrawDefaultInspector();
    }

    
}
#endif
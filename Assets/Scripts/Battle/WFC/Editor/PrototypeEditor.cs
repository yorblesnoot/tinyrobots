using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ModulePrototype))]
public class PrototypeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ModulePrototype prototype = (ModulePrototype)target;
        if (GUILayout.Button("Snap To Grid"))
        {
            prototype.gameObject.transform.position = Vector3Int.RoundToInt(prototype.gameObject.transform.position);
        }
        DrawDefaultInspector();
    }
}


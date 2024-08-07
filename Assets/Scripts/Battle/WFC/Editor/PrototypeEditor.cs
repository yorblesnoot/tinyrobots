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
            Vector3Int cleaned = Vector3Int.FloorToInt(prototype.gameObject.transform.position);
            Vector3 final = cleaned + Vector3.one * .5f;
            prototype.gameObject.transform.position = final;
        }
        DrawDefaultInspector();
    }
}


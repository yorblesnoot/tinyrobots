using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ModuleDefiner))]
public class DefinerEditor : Editor
{
    public override void OnInspectorGUI()
    {

        ModuleDefiner definer = (ModuleDefiner)target;
        if (GUILayout.Button("Generate Connection Rules"))
        {
            definer.DeriveModuleDefinitions();
        }
        DrawDefaultInspector();
    }
}

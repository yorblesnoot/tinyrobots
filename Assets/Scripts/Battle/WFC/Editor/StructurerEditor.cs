using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(AssetStructurer))]
public class StructurerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        AssetStructurer structurer = (AssetStructurer)target;
        if (GUILayout.Button("Restructure Prototype Assets"))
        {
            structurer.ModifyAssets();
        }
        DrawDefaultInspector();
    }
}

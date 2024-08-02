using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BotConverter))]
public class ConverterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        
        BotConverter botConverter = (BotConverter)target;
        if (GUILayout.Button("Populate Libraries"))
        {
            botConverter.CoreLibrary = SerializeAssetsOfType<BotCore>("BotCore");
            botConverter.MutatorLibrary = SerializeAssetsOfType<PartMutator>("PartMutator");
            botConverter.PartLibrary = SerializeAssetsOfType<CraftablePart>("CraftablePart");
        }
        DrawDefaultInspector();
    }

    List<T> SerializeAssetsOfType<T>(string typeName) where T : Object
    {
        string[] guids = AssetDatabase.FindAssets("t:" + typeName);
        string[] paths = guids.Select(g => AssetDatabase.GUIDToAssetPath(g)).ToArray();
        return paths.Select(p => AssetDatabase.LoadAssetAtPath<T>(p)).ToList();
    }
}

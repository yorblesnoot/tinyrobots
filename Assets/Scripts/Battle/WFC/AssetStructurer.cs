using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class AssetStructurer : MonoBehaviour
{
    [SerializeField] string prototypePath = "Assets/WFC/Prototypes";
    readonly int numberOfSpecialpieces = 1;

    public void ModifyAssets()
    {
        
        string[] assets = AssetDatabase.FindAssets("", new string[] { prototypePath }).Select(guid => AssetDatabase.GUIDToAssetPath(guid)).ToArray();
        for (int i = 0; i < assets.Length; i++)
        {
            string path = assets[i];
            int newIndex = i + numberOfSpecialpieces;
            using (var editingScope = new PrefabUtility.EditPrefabContentsScope(path))
            {
                var prefabRoot = editingScope.prefabContentsRoot;
                if (!prefabRoot.TryGetComponent(out ModulePrototype prototype)) continue;
                prototype.PieceIndex = newIndex;
            }
            string currentName = Path.GetFileNameWithoutExtension(Application.dataPath + "/" + path);
            int underscore = currentName.IndexOf("_");
            if(underscore > 0) currentName = currentName[..(underscore - 1)];
            string newName = currentName + "_" + newIndex;
            AssetDatabase.RenameAsset(path, newName);
        }
    }
}

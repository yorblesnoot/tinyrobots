#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class BotBuildButton : MonoBehaviour
{
    [SerializeField] BlueprintControl blueprintControl;
    [SerializeField] string filePath = "Assets/DataSO/RobotRecords/";
    [SerializeField] string fileName = "record";

    readonly string extension = ".asset";
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(GenerateRecord);
    }

    private void GenerateRecord()
    {
        if(!blueprintControl.gameObject.activeInHierarchy) return;
        blueprintControl.BuildBot();
        BotRecord record = ScriptableObject.CreateInstance<BotRecord>();
        record.Record = GUIUtility.systemCopyBuffer;
        string finalPath = filePath + fileName + extension;
        finalPath = AssetDatabase.GenerateUniqueAssetPath(finalPath);
        AssetDatabase.CreateAsset(record, finalPath);
    }
}
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "BotRecord", menuName = "ScriptableObjects/BotRecord")]
public class BotRecord : SOWithGUID
{
    public string displayName;
   [FormerlySerializedAs("record")][TextArea(3,10)] public string Record;
}

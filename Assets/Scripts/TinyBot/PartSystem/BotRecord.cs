using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BotRecord", menuName = "ScriptableObjects/BotRecord")]
public class BotRecord : SOWithGUID
{
    public string displayName;
   [TextArea(3,10)] public string record;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BotCore", menuName = "ScriptableObjects/BotCore")]
public class BotCore : ScriptableObject
{
    public CraftablePart corePart;
    public TreeNode<CraftablePart> bot;
    public BotRecord record;
    //skill tree?
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BotCore", menuName = "ScriptableObjects/BotCore")]
public class BotCore : ScriptableObject
{
    public void Initialize(BotConverter converter)
    {
        bot = converter.StringToBot(StarterRecord.record);
    }

    public TreeNode<CraftablePart> bot;

    [Header("Characteristics")]
    public string displayName;

    [field: SerializeField] public BotRecord StarterRecord {  get; private set; }
    [field: SerializeField] public CraftablePart CorePart { get; private set; }
    //skill tree?
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "BotCore", menuName = "ScriptableObjects/BotCore")]
public class BotCore : ScriptableObject
{
    public void Initialize(BotConverter converter = null)
    {
        if(StarterRecord != null && converter != null) bot = converter.StringToBot(StarterRecord.record);
        ModdedCore = new(corePart);
        ModdedCore.InitializePart();
    }

    public TreeNode<ModdedPart> bot;
    public int currentHealth;

    [Header("Characteristics")]
    public string displayName;

    [field: SerializeField] public BotRecord StarterRecord {  get; private set; }
    [SerializeField] CraftablePart corePart;

    [HideInInspector] public ModdedPart ModdedCore;

    //skill tree?
}

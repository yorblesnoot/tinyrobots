using UnityEngine;

[CreateAssetMenu(fileName = "BotCore", menuName = "ScriptableObjects/BotCore")]
public class BotCore : ScriptableObject
{
    public void Initialize(BotConverter converter = null)
    {
        if(StarterRecord != null && converter != null) Bot = converter.StringToBot(StarterRecord.record);
        ModdedCore = new(corePart);
        ModdedCore.InitializePart();
    }

    public TreeNode<ModdedPart> Bot;
    public float HealthRatio = 1;
    [HideInInspector] public bool Deployable = true;
    [HideInInspector] public ModdedPart ModdedCore;

    [Header("Characteristics")]
    public string displayName;

    [field: SerializeField] public BotRecord StarterRecord {  get; private set; }
    [SerializeField] CraftablePart corePart;

    

    //skill tree?
}

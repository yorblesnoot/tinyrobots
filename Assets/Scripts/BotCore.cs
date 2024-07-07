using UnityEngine;
using UnityEngine.Events;

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

    float _healthRatio = 1f;
    public float HealthRatio { 
        get { return _healthRatio; } 
        set { HealthRatioChanged?.Invoke(); _healthRatio = value; } 
    }
    [HideInInspector] public UnityEvent HealthRatioChanged = new();

    public Sprite CharacterPortrait;
    [HideInInspector] public bool Deployable = true;
    [HideInInspector] public ModdedPart ModdedCore;

    [Header("Characteristics")]
    public string displayName;

    [field: SerializeField] public BotRecord StarterRecord {  get; private set; }
    [SerializeField] CraftablePart corePart;

    

    //skill tree?
}

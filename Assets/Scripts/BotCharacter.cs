using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "BotCore", menuName = "ScriptableObjects/BotCore")]
public class BotCharacter : SOWithGUID
{
    public void Initialize()
    {
        ModdedCore = new(corePart);
    }

    public TreeNode<ModdedPart> Bot;
    public readonly int EnergyCapacity = 100;
    float healthRatio = 1f;
    public float HealthRatio { 
        get { return healthRatio; } 
        set {  healthRatio = value; HealthRatioChanged?.Invoke(); } 
    }
    [HideInInspector] public UnityEvent HealthRatioChanged = new();

    public Sprite CharacterPortrait;
    [HideInInspector] public bool Energized = true;
    [HideInInspector] public ModdedPart ModdedCore;

    [Header("Characteristics")]
    public string DisplayName;
    public bool Playable = true;
    [TextArea(3, 10)] public string Description;

    [field: SerializeField] public BotRecord StarterRecord {  get; private set; }
    [SerializeField] CraftablePart corePart;

    

    //skill tree?
}

using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "BotCore", menuName = "ScriptableObjects/BotCore")]
public class BotCore : SOWithGUID
{
    public void Initialize()
    {
        ModdedCore = new(corePart);
        ModdedCore.InitializePart();
    }

    public TreeNode<ModdedPart> Bot;

    float _healthRatio = 1f;
    public float HealthRatio { 
        get { return _healthRatio; } 
        set {  _healthRatio = value; HealthRatioChanged?.Invoke(); } 
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

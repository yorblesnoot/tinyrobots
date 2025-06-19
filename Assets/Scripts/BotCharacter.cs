using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "BotCharacter", menuName = "ScriptableObjects/BotCharacter")]
public class BotCharacter : SOWithGUID
{
    public void Initialize()
    {
        ModdedCore = new(corePart);
    }

    public TreeNode<ModdedPart> Bot;
    public readonly int EnergyCapacity = 100;
    public string CoreName => DisplayName == "" ? name.Replace("Character", "") : DisplayName;


    public Observable<float> HealthRatio = new() { Value = 1.0f };
    public Observable<int> Mana = new();

    public Sprite CharacterPortrait;
    [HideInInspector] public ModdedPart ModdedCore;

    [Header("Characteristics")]
    public string DisplayName;
    public bool Playable = true;
    [TextArea(3, 10)] public string Description;

    [field: SerializeField] public BotRecord StarterRecord {  get; private set; }
    [SerializeField] CraftablePart corePart;

    //skill tree?
}

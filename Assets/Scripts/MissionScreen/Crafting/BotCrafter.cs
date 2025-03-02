using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BotCrafter : MonoBehaviour
{
    [SerializeField] CraftablePart empty;
    [SerializeField] UnitTab[] tabs;
    [SerializeField] CraftBotStatsDisplay unitStatsDisplay;
    [SerializeField] TMP_Text nameDisplay;
    [SerializeField] GameObject navUI;
    [SerializeField] CinemachineVirtualCamera craftCam;
    public PartSlot OriginSlot;
    public static BotCrafter Instance;

    public GameObject NewSlot;
    public VisualizedPartInventory PartInventory;


    PlayerData playerData;
    


    [HideInInspector] public static BotCharacter ActiveCore { get; private set; }
    public void Initialize()
    {
        Instance = this;
        playerData = SceneGlobals.PlayerData;
        foreach (var part in SceneGlobals.PlayerData.PartInventory) part.InitializePart();
        foreach (var core in SceneGlobals.PlayerData.CoreInventory) core.Initialize();
        playerData.LoadDefaultInventory();
        
        int coreCount = playerData.CoreInventory.Count;
        for (int i = 0; i < tabs.Length; i++)
        {
            bool validTab = i < coreCount;
            tabs[i].gameObject.SetActive(validTab);
            if (!validTab) continue;
            BotCharacter core = playerData.CoreInventory[i];
            tabs[i].AssignTab(() => SwitchCharacter(core), core);
        }
        PartInventory.PartActivated.AddListener(HideUnusableSlots);
        PartInventory.Initialize(SceneGlobals.PlayerData.PartInventory);
        OriginSlot.gameObject.SetActive(true);
    }
    private void OnEnable()
    {
        craftCam.Priority = 100;
    }

    private void OnDisable()
    {
        SaveActiveBotToCore();
        ActiveCore = null;
        craftCam.Priority = 0;
    }

    public void Enable(BotCharacter core)
    {
        navUI.SetActive(false);
        unitStatsDisplay.Initialize();
        gameObject.SetActive(true);
        SwitchCharacter(core);
    }

    void SwitchCharacter(BotCharacter newCore)
    {
        if (newCore == ActiveCore) return;
        SaveActiveBotToCore();
        ActiveCore = newCore;
        nameDisplay.text = newCore.GetCoreName();
        
        if (newCore.Bot != null) PlacePartsInSlots(newCore.Bot, OriginSlot);
        unitStatsDisplay.RefreshDisplays();
        HideUnusableSlots(PartInventory.ActivePart);
        PartInventory.UpdatePartDisplays();
        HighlightTabs();
    }

    void HighlightTabs()
    {
        int activeIndex = SceneGlobals.PlayerData.CoreInventory.IndexOf(ActiveCore);
        for(int i = 0; i < tabs.Length; i++)
        {
            tabs[i].Highlight(i == activeIndex);
        }
    }

    private void SaveActiveBotToCore()
    {
        if (ActiveCore == null) return;

        ActiveCore.Energized = unitStatsDisplay.IsDeployable();
        ActiveCore.Bot = BuildBot();
        OriginSlot.ClearPartIdentity(false, false);
        SaveContainer.SaveGame(SceneGlobals.PlayerData);
    }

    void PlacePartsInSlots(TreeNode<ModdedPart> node, PartSlot slot)
    {
        if(node.Value.BasePart == empty) return;
        PartSlot[] childSlots = slot.SetPartIdentity(node.Value);
        if(childSlots == null || childSlots.Length == 0) return;
        for(int i = 0; i < childSlots.Length && i < node.Children.Count; i++)
        {
            PlacePartsInSlots(node.Children[i], childSlots[i]);
        }
    }

    public TreeNode<ModdedPart> BuildBot()
    {
        var partTree = OriginSlot.BuildTree();
        GUIUtility.systemCopyBuffer = BotConverter.BotToString(partTree);
        return partTree;
    }

    public static Vector3 GetCameraForward()
    {
        return Instance.craftCam.transform.forward;
    }

    

    private void Awake()
    {
        Instance = this;
    }

    public static void HideUnusableSlots(ModdedPart active)
    {
        //Debug.Log(active);
        Instance.OriginSlot.Traverse(HideIfIncompatible);

        void HideIfIncompatible(PartSlot slot)
        {
            if (active == null) slot.Hide(false);
            else if (active.BasePart.PrimaryLocomotion && PartSlot.PrimaryLocomotionSlotted) slot.Hide(true);
            else slot.Hide(!PartSlot.PartCanSlot(active.BasePart.Type, slot.SlotType));
        }
    }
}

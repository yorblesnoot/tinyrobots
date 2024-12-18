using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitSwitcher : MonoBehaviour
{
    [SerializeField] BlueprintControl blueprintControl;
    [SerializeField] CraftablePart empty;
    [SerializeField] UnitTab[] tabs;
    [SerializeField] CraftBotStatsDisplay unitStatsDisplay;
    [SerializeField] TMP_Text nameDisplay;
    [SerializeField] GameObject navUI;

    PlayerData playerData;

    [HideInInspector] public static BotCharacter ActiveCore { get; private set; }
    public void Initialize()
    {
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
        blueprintControl.Initialize(); 
    }

    private void OnDisable()
    {
        SaveActiveBotToCore();
        ActiveCore = null;
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
        
        if (newCore.Bot != null) PlacePartsInSlots(newCore.Bot, blueprintControl.OriginSlot);
        unitStatsDisplay.RefreshDisplays();
        BlueprintControl.HideUnusableSlots();
        blueprintControl.UpdatePartDisplays();
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
        ActiveCore.Bot = blueprintControl.BuildBot();
        blueprintControl.OriginSlot.ClearPartIdentity(false, false);
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
}

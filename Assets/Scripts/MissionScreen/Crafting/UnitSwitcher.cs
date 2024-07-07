using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitSwitcher : MonoBehaviour
{
    [SerializeField] BlueprintControl blueprintControl;
    [SerializeField] CraftablePart empty;
    [SerializeField] UnitTab[] tabs;
    [SerializeField] UnitStatsDisplay unitStatsDisplay;
    [SerializeField] TMP_Text nameDisplay;
    [SerializeField] GameObject navUI;

    PlayerData playerData;

    [HideInInspector] public static BotCore ActiveCore { get; private set; }
    private void Awake()
    {
        playerData = blueprintControl.PlayerData;
        playerData.LoadRecords();
        int coreCount = playerData.CoreInventory.Count;
        for (int i = 0; i < tabs.Length; i++)
        {
            bool validTab = i < coreCount;
            tabs[i].gameObject.SetActive(validTab);
            if (!validTab) continue;
            BotCore core = playerData.CoreInventory[i];
            tabs[i].AssignTab(() => SwitchCharacter(core), core);
        }
    }

    private void OnDisable()
    {
        SaveActiveBotToCore();
        ActiveCore = null;
    }

    public void Enable(BotCore core)
    {
        navUI.SetActive(false);
        unitStatsDisplay.Initialize();
        gameObject.SetActive(true);
        SwitchCharacter(core);
    }

    void SwitchCharacter(BotCore newCore)
    {
        if (newCore == ActiveCore) return;
        SaveActiveBotToCore();
        ActiveCore = newCore;
        blueprintControl.originPart = newCore.ModdedCore;

        nameDisplay.text = UnitTab.GetCoreName(newCore);
        if (newCore.Bot != null)
            PlacePartsInSlots(newCore.Bot.Children[0], blueprintControl.OriginSlot);
        unitStatsDisplay.RefreshDisplays();
        
    }

    private void SaveActiveBotToCore()
    {
        if (ActiveCore == null) return;

        ActiveCore.Deployable = unitStatsDisplay.IsDeployable();
        ActiveCore.Bot = blueprintControl.BuildBot();
        blueprintControl.OriginSlot.ClearPartIdentity(false, false);
    }

    void PlacePartsInSlots(TreeNode<ModdedPart> node, PartSlot slot)
    {
        if(node.Value.BasePart == empty) return;
        PartSlot[] childSlots = slot.SetPartIdentity(node.Value);
        if(childSlots == null || childSlots.Length == 0) return;
        for(int i = 0; i < childSlots.Length; i++)
        {
            PlacePartsInSlots(node.Children[i], childSlots[i]);
        }
    }
}

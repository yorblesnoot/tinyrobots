using UnityEngine;
using UnityEngine.UI;

public class UnitSwitcher : MonoBehaviour
{
    [SerializeField] GameObject unitTab;
    [SerializeField] BlueprintControl blueprintControl;
    [SerializeField] CraftablePart empty;
    [SerializeField] UnitTab[] tabs;

    PlayerData playerData;
    int activeCharacter = -1;

    private void Awake()
    {
        playerData = blueprintControl.PlayerData;
        playerData.LoadRecords();
        int coreCount = playerData.CoreInventory.Count;
        for (int i = 0; i < tabs.Length; i++)
        {
            bool validTab = i < coreCount;
            tabs[i].gameObject.SetActive(validTab);
            int index = i;
            if (validTab) tabs[i].AssignTab(() => SwitchCharacter(index), playerData.CoreInventory[i]);
        }
    }
    private void OnEnable()
    {
        SwitchCharacter(0);
    }

    private void OnDisable()
    {
        SaveActiveBotToCore();
        activeCharacter = -1;
    }

    void SwitchCharacter(int charIndex)
    {
        if (charIndex == activeCharacter) return;
        SaveActiveBotToCore();
        activeCharacter = charIndex;
        blueprintControl.originPart = new(playerData.CoreInventory[activeCharacter].CorePart);

        BotCore core = playerData.CoreInventory[charIndex];
        if (core.bot == null) return;
        PlacePartsInSlots(playerData.CoreInventory[charIndex].bot.Children[0], blueprintControl.OriginSlot);
        
    }

    private void SaveActiveBotToCore()
    {
        if (activeCharacter < 0) return;

        playerData.CoreInventory[activeCharacter].bot = blueprintControl.BuildBot();
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

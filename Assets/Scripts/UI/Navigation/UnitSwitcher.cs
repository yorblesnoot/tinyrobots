using Cinemachine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UnitSwitcher : MonoBehaviour
{
    [SerializeField] GameObject unitTab;
    [SerializeField] BlueprintControl blueprintControl;
    [SerializeField] PlayerData playerData;
    [SerializeField] CraftablePart empty;

    int activeCharacter = -1;

    private void Awake()
    {
        playerData.LoadRecords();
        for (int i = 0; i < playerData.coreInventory.Count; i++)
        {
            GameObject spawned = Instantiate(unitTab, transform);
            AssignSwitch(i, spawned.GetComponentInChildren<Button>());
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

    void AssignSwitch(int index, Button button)
    {
        button.onClick.AddListener(() => SwitchCharacter(index));
    }

    void SwitchCharacter(int charIndex)
    {
        if (charIndex == activeCharacter) return;
        SaveActiveBotToCore();

        activeCharacter = charIndex;
        blueprintControl.originPart = new(playerData.coreInventory[activeCharacter].CorePart);

        if (charIndex >= playerData.coreInventory.Count) return;

        PlacePartsInSlots(playerData.coreInventory[charIndex].bot.Children[0], blueprintControl.OriginSlot);

    }

    private void SaveActiveBotToCore()
    {
        if (activeCharacter < 0) return;

        TreeNode<ModdedPart> bot = blueprintControl.BuildBot();
        playerData.coreInventory[activeCharacter].bot = bot;
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

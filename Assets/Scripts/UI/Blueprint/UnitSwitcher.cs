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
    List<CraftablePart> cores;

    private void Start()
    {
        playerData.LoadRecords();
        cores = playerData.partInventory.Where(part => part.type == PartType.CORE).ToList();
        for(int i = 0; i < cores.Count; i++)
        {
            GameObject spawned = Instantiate(unitTab, transform);
            AssignSwitch(i, spawned.GetComponentInChildren<Button>());
        }
        SwitchCharacter(0);
    }

    void AssignSwitch(int index, Button button)
    {
        button.onClick.AddListener(() => SwitchCharacter(index));
    }

    void SwitchCharacter(int charIndex)
    {
        if(activeCharacter >= 0)
        {
            TreeNode<CraftablePart> bot = blueprintControl.BuildBot();
            playerData.botsInventory[activeCharacter] = bot;
            blueprintControl.OriginSlot.ClearPartIdentity();
        }

        activeCharacter = charIndex;
        blueprintControl.originPart = cores[activeCharacter];

        if (charIndex >= playerData.botsInventory.Count) return;

        PlacePartsInSlots(playerData.botsInventory[charIndex].Children[0], blueprintControl.OriginSlot);

    }

    void PlacePartsInSlots(TreeNode<CraftablePart> node, PartSlot slot)
    {
        if(node.Value == empty) return;
        PartSlot[] childSlots = slot.SetPartIdentity(node.Value);
        if(childSlots == null || childSlots.Length == 0) return;
        for(int i = 0; i < childSlots.Length; i++)
        {
            PlacePartsInSlots(node.Children[i], childSlots[i]);
        }
    }
}

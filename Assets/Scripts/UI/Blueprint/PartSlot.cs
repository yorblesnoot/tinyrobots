using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartSlot : MonoBehaviour
{
    [SerializeField] Button slotButton;
    [SerializeField] GameObject activeIndicator;
    [SerializeField] TMP_Text activePartName;
    [SerializeField] CraftablePart empty;
    [SerializeField] float diagramScaling = 300;

    public PartType slotType;

    CraftablePart partIdentity;
    PartSlot[] childSlots;
    

    private void Awake()
    {
        slotButton.onClick.AddListener(CheckToActivate);
    }

    void CheckToActivate()
    {
        if (partIdentity != null)
        {
            ClearPartIdentity(false, true);
            return;
        }

        if(BlueprintControl.ActivePart == null) return;
        PartType partType = BlueprintControl.ActivePart.type;
        if (partType == slotType) SlotPart();
        else if (partType == PartType.LATERAL)
        {
            if (slotType == PartType.UPPER || slotType == PartType.LOWER) SlotPart();
        }
    }

    void SlotPart()
    {
        SetPartIdentity(BlueprintControl.ActivePart);
        BlueprintControl.SlotActivePart();
    }

    public void ClearPartIdentity(bool destroy, bool toInventory)
    {
        if (partIdentity != null)
        {
            if(toInventory) BlueprintControl.ReturnPart(partIdentity);
            partIdentity = null;
            activeIndicator.SetActive(false);
            activePartName.text = "";

            if (childSlots != null)
            {
                foreach (var slot in childSlots)
                {
                    slot.ClearPartIdentity(true, toInventory);
                }
                childSlots = null;
            }
        }

        if (destroy) Destroy(gameObject);
    }

    public PartSlot[] SetPartIdentity(CraftablePart part)
    {
        childSlots = new PartSlot[part.attachmentPoints.Length];
        partIdentity = part;
        activeIndicator.SetActive(true);
        activePartName.text = part.name;

        for (int i = 0; i < part.attachmentPoints.Length; i++)
        {
            GameObject spawned = Instantiate(BlueprintControl.NewSlot);
            spawned.transform.SetParent(transform, false);
            spawned.transform.localPosition = part.slotPositions[i] * diagramScaling;
            childSlots[i] = spawned.GetComponent<PartSlot>();
            childSlots[i].slotType = part.attachmentPoints[i].SlotType;
        }

        return childSlots;
    }

    public void BuildTree(TreeNode<CraftablePart> parent)
    {
        if (partIdentity == null) partIdentity = empty;
        TreeNode<CraftablePart> myNode = parent.AddChild(partIdentity);
        if (childSlots == null) return;
        foreach(var slot in childSlots)
        {
            slot.BuildTree(myNode);
        }
    }
}

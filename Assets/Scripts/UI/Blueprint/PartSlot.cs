using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartSlot : MonoBehaviour
{
    [SerializeField] Button slotButton;
    [SerializeField] GameObject activeIndicator;
    [SerializeField] TMP_Text activePartName;

    CraftablePart partIdentity;
    PartSlot[] childSlots;
    

    private void Awake()
    {
        BlueprintControl.submitPartTree.AddListener(SubmitChildSlots);
        slotButton.onClick.AddListener(CheckToActivate);
    }

    void CheckToActivate()
    {
        if (partIdentity == null) SetPartIdentity(BlueprintControl.ActivePart);
        else ClearPartIdentity();
    }

    private void ClearPartIdentity()
    {
        partIdentity = null;
        activeIndicator.SetActive(false);
        activePartName.text = "";
        if (childSlots == null) return;

        foreach (var slot in childSlots)
        {
            Destroy(slot.gameObject);
        }
        childSlots = null;
    }


    //replace this with something better
    float screenScalingFactor = 100;
    void SetPartIdentity(CraftablePart part)
    {
        childSlots = new PartSlot[part.attachmentPoints.Length];
        partIdentity = part;
        activeIndicator.SetActive(true);
        activePartName.text = part.name;

        for (int i = 0; i < part.attachmentPoints.Length; i++)
        {
            GameObject spawned = Instantiate(BlueprintControl.NewSlot);
            spawned.transform.SetParent(transform, false);
            spawned.transform.localPosition = part.slotPositions[i] * screenScalingFactor;
            childSlots[i] = spawned.GetComponent<PartSlot>();
        }
    }

    void SubmitChildSlots(SimpleTree<CraftablePart> tree)
    {
        if(partIdentity == null) return;
        tree.AddNode(partIdentity);
        tree.AddChildren(partIdentity, childSlots.Where(x => x.partIdentity != null).Select(x => x.partIdentity).ToList());
    }
}

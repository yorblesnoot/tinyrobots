using TMPro;
using UnityEngine;

public class PartSlot : MonoBehaviour
{
    [SerializeField] Animator slotAnimator;
    [SerializeField] GameObject activeIndicator;
    //[SerializeField] TMP_Text activePartName;
    [SerializeField] CraftablePart empty;
    [SerializeField] float cameraApproachDistance = 3f;

    [HideInInspector] public AttachmentPoint attachmentPoint;
    GameObject mockup;

    CraftablePart partIdentity;
    PartSlot[] childSlots;
    readonly string contract = "contract";
    private void Start()
    {
        Vector3 camPosition = Camera.main.transform.position;
        activeIndicator.transform.LookAt(camPosition);
        Vector3 direction = transform.forward;
        direction.Normalize();
        direction *= cameraApproachDistance;
        activeIndicator.transform.position += direction;
    }
    private void OnMouseDown()
    {
        Debug.Log("clicked");
        CheckToActivate();
    }
    void CheckToActivate()
    {
        if (partIdentity != null)
        {
            ClearPartIdentity(false, true);
            return;
        }

        PartType slotType = attachmentPoint == null ? PartType.CHASSIS : attachmentPoint.SlotType;
        if(BlueprintControl.ActivePart == null) return;
        Debug.Log(BlueprintControl.ActivePart.type + " into " + slotType);
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
        slotAnimator.SetBool(contract, false);
        if (partIdentity != null)
        {
            if(toInventory) BlueprintControl.ReturnPart(partIdentity);
            Destroy(mockup);
            partIdentity = null;
            //activeIndicator.SetActive(false);
            //activePartName.text = "";

            if (childSlots != null)
            {
                foreach (var slot in childSlots)
                {
                    slot.ClearPartIdentity(true, toInventory);
                }
                childSlots = null;
            }
        }

        if (destroy)
        {
            
            Destroy(gameObject);
        }
    }

    public PartSlot[] SetPartIdentity(CraftablePart part)
    {
        slotAnimator.SetBool(contract, true);
        mockup = Instantiate(part.attachableObject);
        Animator partAnimator = mockup.GetComponentInChildren<Animator>();
        if (partAnimator != null) partAnimator.speed = 0;
        PartModifier modifier = mockup.GetComponent<PartModifier>();
        if (modifier.mainRenderers != null)
        {
            foreach (Renderer renderer in modifier.mainRenderers)
            {
                //palette.RecolorPart(renderer, allegiance);
            }
        }

        mockup.transform.SetParent(attachmentPoint == null ? transform : attachmentPoint.transform, false);
        mockup.transform.localRotation = Quaternion.identity;
        AttachmentPoint[] attachmentPoints = mockup.GetComponentsInChildren<AttachmentPoint>();
        partIdentity = part;
        //activeIndicator.SetActive(true);
        //activePartName.text = part.name;

        childSlots = new PartSlot[attachmentPoints.Length];
        for (int i = 0; i < attachmentPoints.Length; i++)
        {
            GameObject slot = Instantiate(BlueprintControl.NewSlot);
            slot.transform.position = attachmentPoints[i].transform.position;
            childSlots[i] = slot.GetComponent<PartSlot>();
            childSlots[i].attachmentPoint = attachmentPoints[i];
        }

        return childSlots;
    }

    public void BuildTree(TreeNode<CraftablePart> parent)
    {
        if (partIdentity == null) partIdentity = empty;
        TreeNode<CraftablePart> incomingNode = parent.AddChild(partIdentity);
        if (childSlots == null) return;
        foreach(var slot in childSlots)
        {
            slot.BuildTree(incomingNode);
        }
    }
}

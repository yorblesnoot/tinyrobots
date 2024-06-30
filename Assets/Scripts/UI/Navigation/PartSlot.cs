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

    ModdedPart partIdentity;
    PartSlot[] childSlots;
    readonly string contractionAnimation = "contract";
    private void OnEnable()
    {
        partIdentity = null;
        Debug.Log("identity is " + partIdentity);
        activeIndicator.transform.localPosition = Vector3.zero;
        Vector3 towardsCamera = -BlueprintControl.GetCameraForward();
        activeIndicator.transform.SetPositionAndRotation(transform.position + towardsCamera * cameraApproachDistance, 
            Quaternion.LookRotation(towardsCamera));
    }

    private void OnMouseDown()
    {
        CheckToActivate();
    }
    void CheckToActivate()
    {
        if (partIdentity != null)
        {
            ClearPartIdentity(false, true);
            return;
        }

        SlotType slotType = attachmentPoint == null ? SlotType.CHASSIS : attachmentPoint.SlotType;
        if(BlueprintControl.ActivePart == null) return;
        Debug.Log(BlueprintControl.ActivePart.BasePart.Type + " into " + slotType);
        SlotType partType = BlueprintControl.ActivePart.BasePart.Type;
        if (partType == slotType) SlotPart();
        else if (partType == SlotType.LATERAL)
        {
            if (slotType == SlotType.UPPER || slotType == SlotType.LOWER) SlotPart();
        }
    }

    void SlotPart()
    {
        SetPartIdentity(BlueprintControl.ActivePart);
        BlueprintControl.SlotActivePart();
    }

    public void ClearPartIdentity(bool destroy, bool toInventory)
    {
        if(slotAnimator != null) slotAnimator.SetBool(contractionAnimation, false);
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

    public PartSlot[] SetPartIdentity(ModdedPart part)
    {
        slotAnimator.SetBool(contractionAnimation, true);
        mockup = Instantiate(part.BasePart.AttachableObject);
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

    public void BuildTree(TreeNode<ModdedPart> parent)
    {
        if (partIdentity == null)
        {
            Debug.Log("part identity was null");
            partIdentity = new();
            partIdentity.BasePart = empty;
        }
        
        Debug.Log(partIdentity.BasePart);
        TreeNode<ModdedPart> incomingNode = parent.AddChild(partIdentity);
        if (childSlots == null) return;
        foreach(var slot in childSlots)
        {
            slot.BuildTree(incomingNode);
        }
    }
}

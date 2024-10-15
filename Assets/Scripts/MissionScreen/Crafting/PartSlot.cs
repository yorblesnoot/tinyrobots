using System;
using UnityEngine;
using UnityEngine.Events;

public class PartSlot : MonoBehaviour
{
    [SerializeField] Animator slotAnimator;
    [SerializeField] GameObject activeIndicator;
    [SerializeField] GameObject incompatibleIndicator;
    [SerializeField] CraftablePart empty;
    [SerializeField] float cameraApproachDistance = 3f;

    [HideInInspector] public AttachmentPoint AttachmentPoint;
    GameObject mockup;

    public ModdedPart PartIdentity { get; private set; }
    public SlotType SlotType { get { return AttachmentPoint == null ? SlotType.CHASSIS : AttachmentPoint.SlotType; } }
    PartSlot[] childSlots;
    readonly string contractionAnimation = "contract";

    public static UnityEvent<ModdedPart, bool> SlottedPart = new();
    public static bool PrimaryLocomotionSlotted {get; private set;}
    private void OnEnable()
    {
        PartIdentity = null;
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
        if (PartIdentity != null)
        {
            ClearPartIdentity(false, true);
            BlueprintControl.FilterAvailableSlots();
            return;
        }
        
        if(BlueprintControl.ActivePart == null) return;
        if(IsCompatibleWithType(BlueprintControl.ActivePart.BasePart.Type)) SlotPart();
    }

    public bool IsCompatibleWithType(SlotType partType)
    {
        SlotType slotType = SlotType;
        if (partType == slotType) return true;  
        else if (partType == SlotType.LATERAL)
        {
            if (slotType == SlotType.UPPER || slotType == SlotType.LOWER) return true;
        }
        return false;
    }

    void SlotPart()
    {
        ModdedPart activePart = BlueprintControl.ActivePart;
        if (activePart.BasePart.PrimaryLocomotion && PrimaryLocomotionSlotted) return;
        SetPartIdentity(activePart);
        BlueprintControl.SlotActivePart();
    }

    public void ClearPartIdentity(bool destroy, bool toInventory)
    {
        SlottedPart.Invoke(PartIdentity, false);
        if(slotAnimator != null) slotAnimator.SetBool(contractionAnimation, false);
        if (PartIdentity != null)
        {
            if(toInventory) BlueprintControl.ReturnPart(PartIdentity);
            if(mockup != null) mockup.SetActive(false);
            if (PartIdentity.BasePart.PrimaryLocomotion) PrimaryLocomotionSlotted = false;
            PartIdentity = null;

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
        PrimaryLocomotionSlotted |= part.BasePart.PrimaryLocomotion;
        SlottedPart.Invoke(part, true);
        slotAnimator.SetBool(contractionAnimation, true);
        part.InstantiateSample();
        mockup = part.Sample;
        if (mockup.TryGetComponent(out Collider collider)) collider.enabled = false;
        mockup.SetActive(true);
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

        mockup.transform.SetParent(AttachmentPoint == null ? transform : AttachmentPoint.transform, false);
        mockup.transform.localRotation = Quaternion.identity;
        AttachmentPoint[] attachmentPoints = mockup.GetComponentsInChildren<AttachmentPoint>();
        PartIdentity = part;

        childSlots = new PartSlot[attachmentPoints.Length];
        for (int i = 0; i < attachmentPoints.Length; i++)
        {
            GameObject slot = Instantiate(BlueprintControl.NewSlot);
            slot.transform.position = attachmentPoints[i].transform.position;
            childSlots[i] = slot.GetComponent<PartSlot>();
            childSlots[i].AttachmentPoint = attachmentPoints[i];
        }

        return childSlots;
    }

    public void BuildTree(TreeNode<ModdedPart> parent)
    {
        if (PartIdentity == null)
        {
            PartIdentity = new();
            PartIdentity.BasePart = empty;
        }
        
        TreeNode<ModdedPart> incomingNode = parent.AddChild(PartIdentity);
        if (childSlots == null) return;
        foreach(var slot in childSlots)
        {
            slot.BuildTree(incomingNode);
        }
    }

    public void Traverse(Action<PartSlot> action)
    {
        action(this);
        if (childSlots == null) return;
        foreach (var slot in childSlots)
        {
            slot.Traverse(action);
        }
    }

    public void Hide(bool hidden = true)
    {
        incompatibleIndicator.SetActive(hidden && PartIdentity == null);
    }
}

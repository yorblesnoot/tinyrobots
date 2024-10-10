using System;
using System.Linq;
using TMPro;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PartSlot : MonoBehaviour
{
    [SerializeField] Animator slotAnimator;
    [SerializeField] GameObject activeIndicator;
    [SerializeField] GameObject incompatibleIndicator;
    [SerializeField] CraftablePart empty;
    [SerializeField] float cameraApproachDistance = 3f;

    [HideInInspector] public AttachmentPoint AttachmentPoint;
    GameObject mockup;

    ModdedPart partIdentity;
    PartSlot[] childSlots;
    readonly string contractionAnimation = "contract";

    public static UnityEvent<ModdedPart, bool> SlottedPart = new();
    static bool primaryLocomotionSlotted;
    private void OnEnable()
    {
        partIdentity = null;
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
            BlueprintControl.FilterAvailableSlots();
            return;
        }
        
        if(BlueprintControl.ActivePart == null) return;
        if(IsCompatibleWithType(BlueprintControl.ActivePart.BasePart.Type)) SlotPart();
    }

    public bool IsCompatibleWithType(SlotType partType)
    {
        SlotType slotType = AttachmentPoint == null ? SlotType.CHASSIS : AttachmentPoint.SlotType;
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
        if (activePart.BasePart.PrimaryLocomotion && primaryLocomotionSlotted) return;
        SetPartIdentity(activePart);
        BlueprintControl.SlotActivePart();
    }

    public void ClearPartIdentity(bool destroy, bool toInventory)
    {
        SlottedPart.Invoke(partIdentity, false);
        if(slotAnimator != null) slotAnimator.SetBool(contractionAnimation, false);
        if (partIdentity != null)
        {
            if(toInventory) BlueprintControl.ReturnPart(partIdentity);
            if(mockup != null) mockup.SetActive(false);
            if (partIdentity.BasePart.PrimaryLocomotion) primaryLocomotionSlotted = false;
            partIdentity = null;

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
        primaryLocomotionSlotted |= part.BasePart.PrimaryLocomotion;
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
        partIdentity = part;

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
        if (partIdentity == null)
        {
            partIdentity = new();
            partIdentity.BasePart = empty;
        }
        
        TreeNode<ModdedPart> incomingNode = parent.AddChild(partIdentity);
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
        incompatibleIndicator.SetActive(hidden && partIdentity == null);
    }
}

using PrimeTween;
using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;

public class PartSlot : MonoBehaviour
{
    [SerializeField] Animator slotAnimator;
    [SerializeField] GameObject activeIndicator;
    [SerializeField] GameObject incompatibleIndicator;
    [SerializeField] CraftablePart empty;
    [SerializeField] float cameraApproachDistance = 3f;
    [SerializeField] string hologramAlphaPropName = "_AlphaMult";
    [SerializeField] float hologramFadeTime = .5f;

    [HideInInspector] public AttachmentPoint AttachmentPoint;
    PartModifier mockup;

    public ModdedPart PartIdentity { get; private set; }
    public SlotType SlotType { get { return AttachmentPoint == null ? SlotType.CHASSIS : AttachmentPoint.SlotType; } }
    PartSlot[] childSlots;
    readonly string contractionAnimation = "contract";
    readonly string pulseAnimation = "pulse";

    public static UnityEvent ModifiedParts = new();
    public static List<ModdedPart> SlottedParts = new();
    public static bool PrimaryLocomotionSlotted {get; private set;}

    static ModdedPart ActivePart {  get { return BotCrafter.Instance.PartInventory.ActivePart; } }
    static VisualizedPartInventory Inventory {  get { return BotCrafter.Instance.PartInventory; } }

    PartLinkDrawer linkDrawer;
    private void Awake()
    {
        linkDrawer = GetComponent<PartLinkDrawer>();
    }

    private void OnEnable()
    {
        PartIdentity = null;
        activeIndicator.transform.localPosition = Vector3.zero;
        Vector3 towardsCamera = -BotCrafter.GetCameraForward();
        activeIndicator.transform.SetPositionAndRotation(ApproachCamera(transform.position), 
            Quaternion.LookRotation(towardsCamera));
    }

    private void OnMouseDown()
    {
        if (PartIdentity != null && PartIdentity.BasePart.Type != SlotType.CORE)
        {
            ClearPartIdentity(false, true);
            BotCrafter.HideUnusableSlots(Inventory.ActivePart);
        }
        else if (ActivePart == null) return;
        else if (PartCanSlot(ActivePart.BasePart.Type, SlotType)) PlayerSlotPart();
    }

    private void OnMouseEnter()
    {
        if (PartIdentity != null) return;
        slotAnimator.SetBool(pulseAnimation, true);
        if (ActivePart == null || !PartCanSlot(ActivePart.BasePart.Type, SlotType)) return;
        DecorateMockup(ActivePart);
        SceneGlobals.BotPalette.RecolorPart(mockup, BotPalette.Special.HOLOGRAM);
        AnimateHologram(true);
    }

    private void OnMouseExit()
    {
        if (PartIdentity != null) return;
        slotAnimator.SetBool(pulseAnimation, false);
        if (mockup == null) return;
        mockup.gameObject.SetActive(false);
        mockup = null;
    }

    public static bool PartCanSlot(SlotType part, SlotType slot)
    {
        if(slot == SlotType.ALL || part == SlotType.ALL) return true;
        if (part == slot) return true;
        else if (part == SlotType.LATERAL)
        {
            if (slot == SlotType.UPPER || slot == SlotType.LOWER) return true;
        }
        return false;
    }

    void PlayerSlotPart()
    {
        if (ActivePart.BasePart.PrimaryLocomotion && PrimaryLocomotionSlotted) return;
        if (!SceneGlobals.PlayerData.DevMode && !ActivePartIsUnderWeightLimit()) return;

        activeSequence.Stop();
        SetPartIdentity(ActivePart);
        BotCrafter.Instance.PartInventory.RemovePart(BotCrafter.Instance.PartInventory.ActivePart);
    }

    public static bool ActivePartIsUnderWeightLimit()
    {
        if(ActivePart == null) return true;
        int totalWeight = 0;
        foreach (ModdedPart part in SlottedParts)
        {
            totalWeight += part.FinalStats[StatType.ENERGY];
        }
        return (totalWeight + ActivePart.FinalStats[StatType.ENERGY] <= BotCrafter.ActiveCore.EnergyCapacity);
    }

    public void ClearPartIdentity(bool destroy, bool toInventory)
    {
        SlottedParts.Remove(PartIdentity);
        
        if (slotAnimator != null && slotAnimator.gameObject.activeSelf) slotAnimator.SetBool(contractionAnimation, false);
        if (PartIdentity != null)
        {
            if (mockup != null)
            {
                mockup.gameObject.SetActive(false);
                mockup.transform.SetParent(null, true);
            }
            if (PartIdentity.BasePart.PrimaryLocomotion) PrimaryLocomotionSlotted = false;

            if (childSlots != null)
            {
                foreach (var slot in childSlots)
                {
                    slot.ClearPartIdentity(true, toInventory);
                }
                childSlots = null;
            }
            //this ordering is to avoid disrupting the smart filter, which checks part identity when the list is updated
            ModdedPart returnPart = PartIdentity;
            PartIdentity = null;
            if (toInventory) Inventory.AddPart(returnPart);
            ModifiedParts.Invoke();
        }
        

        if (destroy)
        {
            Destroy(gameObject);
        }
    }

    

    static Sequence activeSequence;
    void AnimateHologram(bool on)
    {
        //hologram fade is making slotted parts disappear ~~~~~
        if(activeSequence.isAlive) activeSequence.Stop();
        activeSequence = Sequence.Create();
        foreach(Renderer renderer in mockup.mainRenderers)
        {
            foreach(Material material in renderer.materials)
            {
                activeSequence.Group(Tween.MaterialProperty(material, Shader.PropertyToID(hologramAlphaPropName), 
                    startValue: on ? 0 : 1, endValue: on ? 1 : 0, duration: hologramFadeTime));
            }
        }
    }
    

    

    public PartSlot[] SetPartIdentity(ModdedPart part)
    {
        PrimaryLocomotionSlotted |= part.BasePart.PrimaryLocomotion;
        SlottedParts.Add(part);
        
        slotAnimator.SetBool(contractionAnimation, true);
        
        DecorateMockup(part);
        SceneGlobals.BotPalette.RecolorPart(mockup, Allegiance.PLAYER);
        AttachmentPoint[] attachmentPoints = mockup.GetComponentsInChildren<AttachmentPoint>();
        PartIdentity = part;
        

        childSlots = new PartSlot[attachmentPoints.Length];
        for (int i = 0; i < attachmentPoints.Length; i++)
        {
            GameObject slot = Instantiate(BotCrafter.Instance.NewSlot);
            slot.transform.position = attachmentPoints[i].transform.position;
            childSlots[i] = slot.GetComponent<PartSlot>();
            linkDrawer.LinkToSlot(childSlots[i]);
            childSlots[i].AttachmentPoint = attachmentPoints[i];
        }
        ModifiedParts.Invoke();

        return childSlots;
    }

    public Vector3 ApproachCamera(Vector3 point)
    {
        Vector3 towardsCamera = -BotCrafter.GetCameraForward();
        return point + towardsCamera * cameraApproachDistance;
    }

    private void DecorateMockup(ModdedPart part)
    {
        mockup = part.Sample;
        if (mockup.TryGetComponent(out Collider collider)) collider.enabled = false;
        mockup.gameObject.SetActive(true);
        Animator partAnimator = mockup.GetComponentInChildren<Animator>();
        if (partAnimator != null) partAnimator.speed = 0;
        mockup.AttachPart(AttachmentPoint == null ? transform : AttachmentPoint.transform);
    }

    public TreeNode<ModdedPart> BuildTree()
    {
        PartIdentity ??= new(empty);
        TreeNode<ModdedPart> node = new(PartIdentity);
        if (childSlots != null)
            foreach (var slot in childSlots) node.AddChild(slot.BuildTree());
        return node;
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

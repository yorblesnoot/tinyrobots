using PrimeTween;
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
    [SerializeField] string hologramAlphaPropName = "_AlphaMult";
    [SerializeField] float hologramFadeTime = .5f;

    [HideInInspector] public AttachmentPoint AttachmentPoint;
    PartModifier mockup;

    public ModdedPart PartIdentity { get; private set; }
    public SlotType SlotType { get { return AttachmentPoint == null ? SlotType.CHASSIS : AttachmentPoint.SlotType; } }
    PartSlot[] childSlots;
    readonly string contractionAnimation = "contract";

    public static UnityEvent<ModdedPart, bool> SlottedPart = new();
    public static bool PrimaryLocomotionSlotted {get; private set;}

    static ModdedPart activePart {  get { return BotCrafter.Instance.PartInventory.ActivePart; } }
    static VisualizedPartInventory inventory {  get { return BotCrafter.Instance.PartInventory; } }
    private void OnEnable()
    {
        PartIdentity = null;
        activeIndicator.transform.localPosition = Vector3.zero;
        Vector3 towardsCamera = -BotCrafter.GetCameraForward();
        activeIndicator.transform.SetPositionAndRotation(transform.position + towardsCamera * cameraApproachDistance, 
            Quaternion.LookRotation(towardsCamera));
    }

    private void OnMouseDown()
    {
        if (PartIdentity != null && PartIdentity.BasePart.Type != SlotType.CORE)
        {
            ClearPartIdentity(false, true);
            BotCrafter.HideUnusableSlots(inventory.ActivePart);
        }
        else if (activePart == null) return;
        else if (PartCanSlot(activePart.BasePart.Type, SlotType)) PlayerSlotPart();
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
        if (activePart.BasePart.PrimaryLocomotion && PrimaryLocomotionSlotted) return;
        SetPartIdentity(activePart);
        BotCrafter.Instance.PartInventory.RemovePart(BotCrafter.Instance.PartInventory.ActivePart);
    }

    public void ClearPartIdentity(bool destroy, bool toInventory)
    {
        SlottedPart.Invoke(PartIdentity, false);
        if(slotAnimator != null) slotAnimator.SetBool(contractionAnimation, false);
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
            if (toInventory) inventory.AddPart(returnPart);
        }

        if (destroy)
        {
            Destroy(gameObject);
        }
    }

    private void OnMouseEnter()
    {
        if (PartIdentity != null || activePart == null || !PartCanSlot(activePart.BasePart.Type, SlotType)) return;
        DecorateMockup(activePart);
        SceneGlobals.BotPalette.RecolorPart(mockup, BotPalette.Special.HOLOGRAM);
        AnimateHologram(true);
    }

    static Sequence activeSequence;
    void AnimateHologram(bool on)
    {
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
        if (!on) activeSequence.OnComplete(ClearMockup);

        void ClearMockup()
        {
            mockup.gameObject.SetActive(false);
            mockup = null;
        }
    }

    private void OnMouseExit()
    {
        if (mockup == null || PartIdentity != null) return;
        AnimateHologram(false);
    }

    

    public PartSlot[] SetPartIdentity(ModdedPart part)
    {
        PrimaryLocomotionSlotted |= part.BasePart.PrimaryLocomotion;
        SlottedPart.Invoke(part, true);
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
            childSlots[i].AttachmentPoint = attachmentPoints[i];
        }

        return childSlots;
    }

    private void DecorateMockup(ModdedPart part)
    {
        mockup = part.Sample.GetComponent<PartModifier>();
        if (mockup.TryGetComponent(out Collider collider)) collider.enabled = false;
        mockup.gameObject.SetActive(true);
        Animator partAnimator = mockup.GetComponentInChildren<Animator>();
        if (partAnimator != null) partAnimator.speed = 0;
        mockup.transform.SetParent(AttachmentPoint == null ? transform : AttachmentPoint.transform, false);
        mockup.transform.localPosition = Vector3.zero;
        mockup.transform.localRotation = Quaternion.identity;
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

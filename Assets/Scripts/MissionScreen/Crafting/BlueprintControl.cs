using Cinemachine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class BlueprintControl : MonoBehaviour
{
    public static ModdedPart ActivePart;
    public static GameObject NewSlot;
    [HideInInspector] public ModdedPart OriginPart;

    [SerializeField] Color selectedFilterColor = Color.blue;
    Color unselectedFilterColor;

    [Header("Components")]
    [SerializeField] GameObject newSlot;
    [SerializeField] CinemachineVirtualCamera craftCam;
    [SerializeField] List<ActivatablePart> partDisplays;
    [SerializeField] PartOverviewPanel partOverviewPanel;
    [SerializeField] List<FilterButton> filters;
    [SerializeField] SmartFilter smartFilter;
    public PartSlot OriginSlot;

    static BlueprintControl instance;
    
    List<FilterButton> allFilters;
    FilterButton activeFilter;

    public void Initialize()
    {
        activeFilter = filters[0];
        unselectedFilterColor = smartFilter.Button.image.color;
        instance = this;
        allFilters = new(filters)
        {
            smartFilter
        };
        
        NewSlot = newSlot;
        foreach (var filter in allFilters) filter.Button.onClick.AddListener(() => ApplyInventoryFilter(filter));
    }

    private void OnEnable()
    {
        craftCam.Priority = 100;
    }

    private void OnDisable()
    {
        craftCam.Priority = 0;
    }

    static void SetActivePart(ModdedPart part)
    {
        ActivePart = part;
        instance.partOverviewPanel.Become(part);
        HideUnusableSlots();
    }

    public static void HideUnusableSlots()
    {
        instance.OriginSlot.Traverse(HideIfIncompatible);

        void HideIfIncompatible(PartSlot slot)
        {
            if(ActivePart == null) slot.Hide(false);
            else if(ActivePart.BasePart.PrimaryLocomotion && PartSlot.PrimaryLocomotionSlotted) slot.Hide(true);
            else slot.Hide(!PartSlot.PartCanSlot(ActivePart.BasePart.Type, slot.SlotType));
        }
    }

    public static void ConsumeActivePart()
    {

        if (!SceneGlobals.PlayerData.DevMode) SceneGlobals.PlayerData.PartInventory.Remove(ActivePart);
        SetActivePart(null);

        ActivatablePart.resetActivation.Invoke();
        instance.UpdatePartDisplays();
    }

    public static void ReturnPart(ModdedPart part)
    {
        if (!SceneGlobals.PlayerData.DevMode) SceneGlobals.PlayerData.PartInventory.Add(part);
        instance.UpdatePartDisplays();
        
    }

    void ApplyInventoryFilter(FilterButton filter)
    {
        activeFilter = filter;
        UpdatePartDisplays();
    }

    public void UpdatePartDisplays()
    {
        List<ModdedPart> filteredParts = activeFilter.FilterParts(SceneGlobals.PlayerData.PartInventory).OrderBy(part=> part.BasePart.name).ToList();
        foreach(var filter in allFilters) filter.Button.image.color = filter == activeFilter ? selectedFilterColor : unselectedFilterColor;
        for (int i = 0; i < filteredParts.Count; i++)
        {
            if(i == partDisplays.Count - 1)
            {
                ActivatablePart display = Instantiate(partDisplays[^1], partDisplays[^1].transform.parent);
                partDisplays.Add(display);
            }
            partDisplays[i].gameObject.SetActive(true);
            ModdedPart part = filteredParts[i];
            partDisplays[i].DisplayPart(part, SetActivePart);
            partDisplays[i].SetTextColor(part.Rarity.TextColor);
        }

        for(int i = filteredParts.Count; i < partDisplays.Count; i++) partDisplays[i].gameObject.SetActive(false);
    }

    public TreeNode<ModdedPart> BuildBot()
    {
        var partTree = OriginSlot.BuildTree();
        GUIUtility.systemCopyBuffer = BotConverter.BotToString(partTree);
        return partTree;
    }

    public static Vector3 GetCameraForward()
    {
        return instance.craftCam.transform.forward;
    }
}

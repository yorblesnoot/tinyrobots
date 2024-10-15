using Cinemachine;
using System.Collections.Generic;
using UnityEngine;


public class BlueprintControl : MonoBehaviour
{
    public static ModdedPart ActivePart;
    public static GameObject NewSlot;
    [HideInInspector] public ModdedPart OriginPart;

    [SerializeField] GameObject newSlot;
    [SerializeField] CinemachineVirtualCamera craftCam;
    [SerializeField] List<ActivatablePart> partDisplays;
    [SerializeField] PartOverviewPanel partOverviewPanel;
    public PartSlot OriginSlot;


    static BlueprintControl instance;
    static bool devMode;
    

    [SerializeField] List<FilterButton> filters;
    [SerializeField] SmartFilter smartFilter;
    FilterButton activeFilter;

    public void Initialize()
    {
        instance = this;
        List<FilterButton> allFilters = new(filters)
        {
            smartFilter
        };
        foreach (var filter in allFilters) filter.Button.onClick.AddListener(() => ApplyInventoryFilter(filter));
        NewSlot = newSlot;
        foreach (var core in SceneGlobals.PlayerData.CoreInventory) core.Initialize();
    }

    private void OnEnable()
    {
        craftCam.Priority = 100;
        devMode = SceneGlobals.PlayerData.DevMode;
    }

    private void OnDisable()
    {
        craftCam.Priority = 0;
    }

    static void SetActivePart(ModdedPart part)
    {
        ActivePart = part;
        instance.partOverviewPanel.Become(part);
        FilterAvailableSlots();
    }

    public static void FilterAvailableSlots()
    {
        instance.OriginSlot.Traverse(HideIfIncompatible);

        void HideIfIncompatible(PartSlot slot)
        {
            if(ActivePart == null) slot.Hide(false);
            else if(ActivePart.BasePart.PrimaryLocomotion && PartSlot.PrimaryLocomotionSlotted) slot.Hide(true);
            else slot.Hide(!slot.IsCompatibleWithType(ActivePart.BasePart.Type));
        }
    }

    public static void SlotActivePart()
    {

        if (!devMode) SceneGlobals.PlayerData.PartInventory.Remove(ActivePart);
        SetActivePart(null);

        ActivatablePart.resetActivation.Invoke();
        instance.UpdatePartDisplays();
    }

    public static void ReturnPart(ModdedPart part)
    {
        if (!devMode) SceneGlobals.PlayerData.PartInventory.Add(part);
        instance.UpdatePartDisplays();
        
    }

    void ApplyInventoryFilter(FilterButton filter)
    {
        activeFilter = filter;
        UpdatePartDisplays();
    }

    public void UpdatePartDisplays()
    {
        List<ModdedPart> filteredParts = activeFilter != null 
            ? activeFilter.FilterParts(SceneGlobals.PlayerData.PartInventory)
            : SceneGlobals.PlayerData.PartInventory;
        for (int i = 0; i < filteredParts.Count; i++)
        {
            if(i == partDisplays.Count - 1)
            {
                ActivatablePart display = Instantiate(partDisplays[^1], partDisplays[^1].transform.parent);
                partDisplays.Add(display);
            }
            partDisplays[i].gameObject.SetActive(true);
            ModdedPart part = filteredParts[i];
            part.InitializePart();
            partDisplays[i].DisplayPart(part, SetActivePart);
        }

        for(int i = filteredParts.Count; i < partDisplays.Count; i++) partDisplays[i].gameObject.SetActive(false);
    }

    public TreeNode<ModdedPart> BuildBot()
    {
        TreeNode<ModdedPart> partTree = new(OriginPart);
        OriginSlot.BuildTree(partTree);
        
        GUIUtility.systemCopyBuffer = BotConverter.BotToString(partTree);
        return partTree;
    }

    public static Vector3 GetCameraForward()
    {
        return instance.craftCam.transform.forward;
    }
}

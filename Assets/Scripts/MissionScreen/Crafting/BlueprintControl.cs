using Cinemachine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class BlueprintControl : MonoBehaviour
{
    public static ModdedPart ActivePart;
    public static GameObject NewSlot;
    [HideInInspector] public ModdedPart originPart;

    [SerializeField] GameObject newSlot;
    [SerializeField] CinemachineVirtualCamera craftCam;
    [SerializeField] List<ActivatablePart> partDisplays;
    [SerializeField] PartOverviewPanel partOverviewPanel;
    public PartSlot OriginSlot;


    static BlueprintControl instance;
    static bool devMode;
    [Serializable]
    class FilterButton
    {
        public Button Button;
        public SlotType Type;
    }

    [SerializeField] List<FilterButton> filters;
    FilterButton activeFilter;

    public void Initialize()
    {
        instance = this;
        foreach (var filter in filters) filter.Button.onClick.AddListener(() => ApplyInventoryFilter(filter));
        NewSlot = newSlot;
        UpdatePartDisplays();
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
            else slot.Hide(!slot.IsCompatibleWithType(ActivePart.BasePart.Type));
        }
    }

    public static void SlotActivePart()
    {
        if (devMode) return;
        SceneGlobals.PlayerData.PartInventory.Remove(ActivePart);
        SetActivePart(null);

        ActivatablePart.resetActivation.Invoke();
        instance.UpdatePartDisplays();
    }

    public static void ReturnPart(ModdedPart part)
    {
        if (devMode) return;
        SceneGlobals.PlayerData.PartInventory.Add(part);
        instance.UpdatePartDisplays();
        
    }

    void ApplyInventoryFilter(FilterButton filter)
    {
        activeFilter = filter;
        UpdatePartDisplays();
    }

    void UpdatePartDisplays()
    {
        List<ModdedPart> filteredParts = activeFilter != null 
            ? SceneGlobals.PlayerData.PartInventory.Where(part => part.BasePart.Type == activeFilter.Type).ToList() 
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
        TreeNode<ModdedPart> partTree = new(originPart);
        OriginSlot.BuildTree(partTree);
        
        GUIUtility.systemCopyBuffer = BotConverter.BotToString(partTree);
        return partTree;
    }

    public static Vector3 GetCameraForward()
    {
        return instance.craftCam.transform.forward;
    }
}

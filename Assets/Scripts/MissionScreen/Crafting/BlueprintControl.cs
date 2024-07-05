using Cinemachine;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
    public PlayerData PlayerData;


    static BlueprintControl Instance;
    static List<ModdedPart> partInventory;
    static bool devMode;


    private void OnEnable()
    {
        craftCam.Priority = 100;
        devMode = PlayerData.DevMode;
    }

    private void OnDisable()
    {
        craftCam.Priority = 0;
    }

    void SetActivePart(ModdedPart part)
    {
        ActivePart = part;
        partOverviewPanel.Become(part);
    }

    public static void SlotActivePart()
    {
        if (devMode) return;
        partInventory.Remove(ActivePart);
        ActivePart = null;
        Instance.partOverviewPanel.Hide();
        ActivatablePart.resetActivation.Invoke();
        Instance.UpdatePartDisplays();
    }

    public static void ReturnPart(ModdedPart part)
    {
        if (devMode) return;
        partInventory.Add(part);
        Instance.UpdatePartDisplays();
    }

    public void Initialize()
    {
        Instance = this;
        partInventory = PlayerData.PartInventory;
        NewSlot = newSlot;
        UpdatePartDisplays();
        foreach (var core in PlayerData.CoreInventory) core.Initialize();
    }

    void UpdatePartDisplays()
    {
        for (int i = 0; i < PlayerData.PartInventory.Count; i++)
        {
            if(i == partDisplays.Count - 1)
            {
                ActivatablePart display = Instantiate(partDisplays[^1], partDisplays[^1].transform.parent);
                partDisplays.Add(display);
            }
            partDisplays[i].gameObject.SetActive(true);
            ModdedPart part = PlayerData.PartInventory[i];
            part.InitializePart();
            partDisplays[i].DisplayPart(part, SetActivePart);
        }

        for(int i = PlayerData.PartInventory.Count; i < partDisplays.Count; i++) partDisplays[i].gameObject.SetActive(false);
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
        return Instance.craftCam.transform.forward;
    }
}

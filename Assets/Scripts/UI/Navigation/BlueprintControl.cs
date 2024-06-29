using Cinemachine;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlueprintControl : MonoBehaviour
{
    public static ModdedPart ActivePart;
    public static GameObject NewSlot;
    [HideInInspector] public ModdedPart originPart;

    [SerializeField] GameObject newSlot;
    [SerializeField] PlayerData playerData;
    [SerializeField] List<ListedPart> partDisplays;
    public PartSlot OriginSlot;
    

    static BlueprintControl Instance;
    static List<ModdedPart> partInventory;
    HashSet<ModdedPart> initializedParts = new();
    [SerializeField] CinemachineVirtualCamera craftCam;
    private void OnEnable()
    {
        craftCam.Priority = 100;
    }

    private void OnDisable()
    {
        craftCam.Priority = 0;
    }

    public static void SetActivePart(ModdedPart part)
    {
        ActivePart = part;
    }

    public static void SlotActivePart()
    {
        partInventory.Remove(ActivePart);
        ActivePart = null;
        ListedPart.resetActivation.Invoke();
        Instance.UpdatePartDisplays();
    }

    public static void ReturnPart(ModdedPart part)
    {
        partInventory.Add(part);
        Instance.UpdatePartDisplays();
    }

    public void Initialize()
    {
        Instance = this;
        partInventory = playerData.partInventory;
        NewSlot = newSlot;
        UpdatePartDisplays();
    }

    void UpdatePartDisplays()
    {
        for (int i = 0; i < playerData.partInventory.Count; i++)
        {
            if(i == partDisplays.Count - 1)
            {
                ListedPart display = Instantiate(partDisplays[^1], partDisplays[^1].transform.parent);
                partDisplays.Add(display);
            }

            if (!initializedParts.Contains(playerData.partInventory[i]))
            {
                playerData.partInventory[i].DeriveAttachmentAttributes();
                initializedParts.Add(playerData.partInventory[i]);
            }
            partDisplays[i].gameObject.SetActive(true);
            partDisplays[i].InitializeDisplay(playerData.partInventory[i], SetActivePart);
        }

        for(int i = playerData.partInventory.Count; i < partDisplays.Count; i++) partDisplays[i].gameObject.SetActive(false);
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

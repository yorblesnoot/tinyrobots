using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BlueprintControl : MonoBehaviour
{
    public static CraftablePart ActivePart;
    public static TreeNode<CraftablePart> activeTree;

    [SerializeField] public static GameObject NewSlot;
    [SerializeField] GameObject newSlot;
    [SerializeField] BotAssembler assembler;
    [SerializeField] CraftablePart originPart;
    [SerializeField] PartSlot originSlot;

    public static void SetActivePart(CraftablePart part)
    {
        ActivePart = part;
    }

    [SerializeField] PlayerData playerData;
    [SerializeField] List<ListedPart> partDisplays;

    private void Awake()
    {
        NewSlot = newSlot;
        for (int i = 0; i < playerData.partInventory.Count; i++)
        {
            InitializePartDisplay(i);
            playerData.partInventory[i].DeriveAttachmentAttributes();
        }
    }

    private void InitializePartDisplay(int i)
    {
        partDisplays[i].gameObject.SetActive(true);
        partDisplays[i].InitializeDisplay(playerData.partInventory[i]);
    }

    public TreeNode<CraftablePart> partTree;
    public void BuildBot()
    {
        partTree = new(originPart);
        originSlot.BuildTree(partTree);
        
        //playerData.bot = partTree;
        assembler.BuildBotFromTree(partTree);
        gameObject.SetActive(false);
    }


}

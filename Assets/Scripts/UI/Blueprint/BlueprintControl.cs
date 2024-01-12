using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GetPartTree : UnityEvent<SimpleTree<CraftablePart>> { }
public class BlueprintControl : MonoBehaviour
{
    public static CraftablePart ActivePart;
    public static GetPartTree submitPartTree = new();

    [SerializeField] public static GameObject NewSlot;
    [SerializeField] GameObject newSlot;
    [SerializeField] BotAssembler assembler;

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

    public SimpleTree<CraftablePart> partTree;
    public void BuildBotTree()
    {
        partTree = new();
        submitPartTree.Invoke(partTree);
        //playerData.bot = partTree;
        assembler.BuildBotFromTree(partTree);
        gameObject.SetActive(false);
    }


}

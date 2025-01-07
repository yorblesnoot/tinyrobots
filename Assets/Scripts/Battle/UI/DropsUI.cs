using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropsUI : MonoBehaviour
{

    [SerializeField] ActivatablePart[] dropDisplays;
    [SerializeField] PartOverviewPanel partPreview;
    [SerializeField] PlayerData playerData;
    [SerializeField] PartGenerator partGenerator;
    [SerializeField] Button continueButton;

    public void ShowDrops()
    {
        gameObject.SetActive(true);
        
        List<ModdedPart> parts = partGenerator.GenerateDropList();
        SceneGlobals.PlayerData.PartInventory.AddRange(parts);

        for (int i = 0; i < dropDisplays.Length; i++)
        {
            bool generate = i < parts.Count;
            dropDisplays[i].gameObject.SetActive(generate);
            if (!generate) continue;
            dropDisplays[i].DisplayPart(parts[i], PreviewPart);
            dropDisplays[i].SetTextColor(parts[i].Rarity.TextColor);
        }
    }

    void PreviewPart(ModdedPart part)
    {
        partPreview.Become(part);
        partPreview.gameObject.SetActive(true);
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D)) ShowDrops();
    }
#endif
}

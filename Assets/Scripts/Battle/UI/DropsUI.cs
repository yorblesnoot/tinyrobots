using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DropsUI : MonoBehaviour
{

    [SerializeField] ActivatablePart[] dropDisplays;
    [SerializeField] PartOverviewPanel partPreview;
    [SerializeField] PlayerData playerData;
    [SerializeField] PartGenerator partGenerator;
    
    [SerializeField] Button continueButton;
    [SerializeField] List<GameObject> otherUI;

    public void ShowDrops(UnityAction doneCallback)
    {
        continueButton.onClick.AddListener(doneCallback);
        gameObject.SetActive(true);
        foreach(var obj in otherUI) obj.gameObject.SetActive(false);
        List<ModdedPart> parts = partGenerator.GenerateDropList();

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

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.D)) ShowDrops(null);
    }
}

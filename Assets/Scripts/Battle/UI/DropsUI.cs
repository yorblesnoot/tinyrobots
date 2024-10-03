using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DropsUI : MonoBehaviour
{
    
    [SerializeField] float partFadeDuration = .5f;

    [SerializeField] ActivatablePart[] dropDisplays;
    [SerializeField] PartOverviewPanel partPreview;
    [SerializeField] PlayerData playerData;
    [SerializeField] PartGenerator partGenerator;
    [SerializeField] PartRarityPalette rarityPalette;
    [SerializeField] Button continueButton;

    [SerializeField] int minDrops = 2;
    [SerializeField] int maxDrops = 4;
    [SerializeField] int maxMods = 3;
    public void ShowDrops(UnityAction doneCallback)
    {
        continueButton.onClick.AddListener(doneCallback);
        gameObject.SetActive(true);
        int dropCount = Random.Range(minDrops, maxDrops);

        for (int i = 0; i < dropDisplays.Length; i++)
        {
            bool generate = i < dropCount;
            dropDisplays[i].gameObject.SetActive(generate);
            if (!generate) continue;
            int modCount = Random.Range(0, maxMods);
            ModdedPart modPart = partGenerator.Generate(modCount);
            playerData.PartInventory.Add(modPart);
            dropDisplays[i].DisplayPart(modPart, PreviewPart);
            dropDisplays[i].SetTextColor(rarityPalette.GetModColor(modCount));
            dropDisplays[i].gameObject.SetActive(true);
        }
    }

    void PreviewPart(ModdedPart part)
    {
        partPreview.Become(part);
        partPreview.gameObject.SetActive(true);
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.D)) OfferDrops(null);
    }
}

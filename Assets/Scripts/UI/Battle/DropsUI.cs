using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DropsUI : MonoBehaviour
{
    [SerializeField] int modNumber = 3;
    [SerializeField] float partFadeDuration = .5f;
    [SerializeField] float missionEndDelay = 2;
    [SerializeField] BotConverter botConverter;
    [SerializeField] DropButtonDetailed[] dropDisplays;
    [SerializeField] PlayerData playerData;
    [SerializeField] PartGenerator partGenerator;

    UnityAction enderCallback;
    public void OfferDrops(UnityAction doneCallback)
    {
        enderCallback = doneCallback;
        gameObject.SetActive(true);
        for(int i = 0; i < dropDisplays.Length; i++)
        {
            ModdedPart modPart = partGenerator.Generate(modNumber);
            dropDisplays[i].DisplayPart(modPart, GivePart);
            Tween.Alpha(dropDisplays[i].group, startValue: 0, endValue: 1, duration: partFadeDuration);
        }
    }

    void GivePart(ModdedPart part)
    {
        playerData.PartInventory.Add(part);
        Sequence sequence = Sequence.Create();
        foreach(var display in dropDisplays)
        {
            display.group.interactable = false;
            if (display.PartIdentity != part) sequence.Group(Tween.Alpha(display.group, endValue: 0, duration: partFadeDuration));
        }
        if(enderCallback != null) 
            sequence.Chain(Tween.Delay(2)).OnComplete(() => enderCallback());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D)) OfferDrops(null);
    }
}

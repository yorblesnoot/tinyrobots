using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DropsUI : MonoBehaviour
{
    [SerializeField] float partFadeDuration = .5f;
    [SerializeField] float missionEndDelay = 2;
    [SerializeField] BotConverter botConverter;
    [SerializeField] ListedPart[] dropDisplays;
    [SerializeField] PlayerData playerData;
    List<CraftablePart> possibleDrops;

    UnityAction enderCallback;
    public void OfferDrops(UnityAction doneCallback)
    {
        enderCallback = doneCallback;
        gameObject.SetActive(true);
        possibleDrops = new(botConverter.PartLibrary);
        for(int i = 0; i < dropDisplays.Length; i++)
        {
            CraftablePart part = possibleDrops.GrabRandomly();
            ModdedPart modPart = new(part);
            //RANDOMLY GENERATE MUTATORS HERE ~~~~~~~~~~~~~~~~~~
            Debug.LogError("random stats are not implemented");
            dropDisplays[i].InitializeDisplay(modPart, GivePart);
            Tween.Alpha(dropDisplays[i].group, startValue: 0, endValue: 1, duration: partFadeDuration);
        }
    }

    void GivePart(ModdedPart part)
    {
        playerData.partInventory.Add(part);
        Sequence sequence = Sequence.Create();
        foreach(var display in dropDisplays)
        {
            display.group.interactable = false;
            if (display.PartIdentity != part) sequence.Group(Tween.Alpha(display.group, endValue: 0, duration: partFadeDuration));
        }
        sequence.Chain(Tween.Delay(2)).OnComplete(() => enderCallback());
    }
}

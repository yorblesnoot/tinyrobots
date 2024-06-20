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
        possibleDrops = new(botConverter.parts);
        for(int i = 0; i < dropDisplays.Length; i++)
        {
            CraftablePart part = possibleDrops.GrabRandomly();
            dropDisplays[i].InitializeDisplay(part, GivePart);
            Tween.Alpha(dropDisplays[i].group, startValue: 0, endValue: 1, duration: partFadeDuration);
        }
    }

    void GivePart(CraftablePart part)
    {
        playerData.partInventory.Add(part);
        Sequence sequence = Sequence.Create();
        foreach(var display in dropDisplays)
        {
            display.group.interactable = false;
            if (display.partIdentity != part) sequence.Group(Tween.Alpha(display.group, endValue: 0, duration: partFadeDuration));
        }
        sequence.Chain(Tween.Delay(2)).OnComplete(() => enderCallback());
    }
}

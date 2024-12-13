using PrimeTween;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ActivatablePart : PartButton
{
    [SerializeField] TMP_Text nameDisplay;
    [SerializeField] TMP_Text weightDisplay;
    [SerializeField] CanvasGroup activationOverlay;
    [SerializeField] PartStatIcon[] statDisplays;
    [SerializeField] float activationFadeTime = .5f;


    public static UnityEvent resetActivation = new();

    UnityAction<ModdedPart> submitPartCallback;
    public override void DisplayPart(ModdedPart part, UnityAction<ModdedPart> activationCallback)
    {
        Group = GetComponent<CanvasGroup>();
        submitPartCallback = activationCallback;
        PartIdentity = part;
        nameDisplay.text = part.BasePart.name;
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(BecomeActive);
        resetActivation.AddListener(BecomeInactive);

        List<StatType> statTypes = part.FinalStats.Keys.ToList();
        statTypes.Remove(StatType.ENERGY);
        weightDisplay.text = part.FinalStats[StatType.ENERGY].ToString();
        
        for (int i = 0; i < statDisplays.Count(); i++)
        {
            if(i < statTypes.Count)
            {
                statDisplays[i].AssignStat(statTypes[i], part.FinalStats[statTypes[i]]);
            }
            else statDisplays[i].Hide();
        }
    }

    public void SetTextColor(Color color)
    {
        nameDisplay.color = color;
    }

    void BecomeActive()
    {
        resetActivation.Invoke();
        Tween.Alpha(activationOverlay, 1, duration: activationFadeTime);
        submitPartCallback(PartIdentity);
    }

    void BecomeInactive()
    {
        Tween.Alpha(activationOverlay, 0, duration: activationFadeTime);
    }
}

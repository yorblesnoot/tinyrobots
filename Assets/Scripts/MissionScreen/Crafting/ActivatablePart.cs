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
    [SerializeField] Image buttonImage;
    [SerializeField] Color activeColor;
    [SerializeField] PartStatIcon[] statDisplays;



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
        for(int i = 0; i < statDisplays.Count(); i++)
        {
            if(i < statTypes.Count)
            {
                statDisplays[i].AssignStat(statTypes[i], part.FinalStats[statTypes[i]]);
            }
            else statDisplays[i].Hide();
        }
        weightDisplay.text = part.Weight.ToString();
    }

    void BecomeActive()
    {
        resetActivation.Invoke();
        buttonImage.color = activeColor;
        submitPartCallback(PartIdentity);
    }

    void BecomeInactive()
    {
        buttonImage.color = Color.white;
    }
}

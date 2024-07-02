using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class VisualizedPart : MonoBehaviour
{
    [SerializeField] TMP_Text nameDisplay;
    [SerializeField] TMP_Text weightDisplay;
    [SerializeField] Button selectButton;
    [SerializeField] Image buttonImage;
    [SerializeField] Color activeColor;
    [SerializeField] PartStatIcon[] statDisplays;

    [HideInInspector] public CanvasGroup group;

    public ModdedPart PartIdentity {get; private set;}

    public static UnityEvent resetActivation = new();

    UnityAction<ModdedPart> submitPartCallback;
    public void InitializeDisplay(ModdedPart part, UnityAction<ModdedPart> activationCallback)
    {
        group = GetComponent<CanvasGroup>();
        submitPartCallback = activationCallback;
        PartIdentity = part;
        nameDisplay.text = part.BasePart.name;
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(BecomeActive);
        resetActivation.AddListener(BecomeInactive);

        List<StatType> statTypes = part.Stats.Keys.ToList();
        for(int i = 0; i < statDisplays.Count(); i++)
        {
            if(i < statTypes.Count)
            {
                statDisplays[i].AssignStat(statTypes[i], part.Stats[statTypes[i]]);
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

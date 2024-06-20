using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ListedPart : MonoBehaviour
{
    [SerializeField] TMP_Text nameDisplay;
    [SerializeField] TMP_Text weightDisplay;
    [SerializeField] Button selectButton;
    [SerializeField] Image buttonImage;
    [SerializeField] Color activeColor;
    [SerializeField] PartStatDisplay[] statDisplays;

    [HideInInspector] public CanvasGroup group;

    public CraftablePart partIdentity {get; private set;}

    public static UnityEvent resetActivation = new();

    UnityAction<CraftablePart> submitPartCallback;
    public void InitializeDisplay(CraftablePart part, UnityAction<CraftablePart> activationCallback)
    {
        group = GetComponent<CanvasGroup>();
        submitPartCallback = activationCallback;
        partIdentity = part;
        nameDisplay.text = part.name;
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
        weightDisplay.text = part.weight.ToString();
    }

    void BecomeActive()
    {
        resetActivation.Invoke();
        buttonImage.color = activeColor;
        submitPartCallback(partIdentity);
    }

    void BecomeInactive()
    {
        buttonImage.color = Color.white;
    }
}

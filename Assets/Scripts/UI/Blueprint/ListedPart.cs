using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ListedPart : MonoBehaviour
{
    [SerializeField] TMP_Text nameDisplay;
    [SerializeField] Button selectButton;
    [SerializeField] Image buttonImage;
    [SerializeField] Color activeColor;

    CraftablePart partIdentity;

    public static UnityEvent resetActivation = new();

    public void InitializeDisplay(CraftablePart part)
    {
        partIdentity = part;
        nameDisplay.text = part.name;
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(BecomeActive);
        resetActivation.AddListener(BecomeInactive);
    }

    void BecomeActive()
    {
        resetActivation.Invoke();
        buttonImage.color = activeColor;
        BlueprintControl.SetActivePart(partIdentity);
    }

    void BecomeInactive()
    {
        buttonImage.color = Color.white;
    }
}

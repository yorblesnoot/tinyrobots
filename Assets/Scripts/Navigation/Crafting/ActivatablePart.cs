using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ActivatablePart : PartButton, IPointerClickHandler
{
    [SerializeField] TMP_Text nameDisplay;
    [SerializeField] TMP_Text weightDisplay;
    [SerializeField] CanvasGroup activationOverlay;
    [SerializeField] float activationFadeTime = .5f;


    public static UnityEvent ResetActivation = new();

    UnityAction<ModdedPart> submitPartCallback;
    UnityAction<ModdedPart> secondaryCallback;
    bool active = false;
    public override void DisplayPart(ModdedPart part, UnityAction<ModdedPart> activationCallback, int value, UnityAction<ModdedPart> secondaryActivation = null)
    {
        Group = GetComponent<CanvasGroup>();
        submitPartCallback = activationCallback;
        secondaryCallback = secondaryActivation;
        PartIdentity = part;
        nameDisplay.text = part.BasePart.name;
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(BecomeActive);
        ResetActivation.AddListener(BecomeInactive);

        weightDisplay.text = value.ToString();
    }

    public void SetTextColor(Color color)
    {
        nameDisplay.color = color;
    }

    void BecomeActive()
    {
        bool previouslyActive = active;
        ResetActivation.Invoke();
        if(previouslyActive) secondaryCallback?.Invoke(PartIdentity);
        else
        {
            Tween.Alpha(activationOverlay, 1, duration: activationFadeTime);
            submitPartCallback(PartIdentity);
        }
        active = !previouslyActive;
    }

    void BecomeInactive()
    {
        //this doesnt disable the part on the inventory's end ~~~~
        active = false;
        if (activationOverlay.alpha == 0) return;
        Tween.Alpha(activationOverlay, 0, duration: activationFadeTime);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Right)
        {
            secondaryCallback(PartIdentity);
        }
    }
}

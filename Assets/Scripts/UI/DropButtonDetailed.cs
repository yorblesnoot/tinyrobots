using UnityEngine;
using UnityEngine.Events;

public class DropButtonDetailed : PartButton
{
    [SerializeField] PartOverviewPanel partPanel;
    public override void DisplayPart(ModdedPart part, UnityAction<ModdedPart> partCallback)
    {
        PartIdentity = part;
        partPanel.Become(part);
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => partCallback(part));
    }
}

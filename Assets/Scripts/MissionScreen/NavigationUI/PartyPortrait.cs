using UnityEngine;
using UnityEngine.UI;

public class PartyPortrait : MonoBehaviour
{
    [SerializeField] Image portrait;
    [SerializeField] Button button;
    [SerializeField] HealthOverlay healthOverlay;
    public void Become(BotCore core, UnitSwitcher switcher)
    {
        portrait.sprite = core.CharacterPortrait;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => switcher.Enable(core));
        healthOverlay.UpdateHealth(Mathf.RoundToInt(core.HealthRatio * 100), 100);
    }
}

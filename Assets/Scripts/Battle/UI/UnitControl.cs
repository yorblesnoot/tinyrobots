using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitControl : MonoBehaviour
{
    [SerializeField] ClickableAbility[] clickableAbilities;
    [SerializeField] UnclickableAbility[] unclickableAbilities;
    [SerializeField] Image unitPortrait;
    [SerializeField] Button turnEnd;

    [SerializeField] TurnManager turnManager;

    [SerializeField] HealthOverlay healthOverlay;

    List<ClickableAbility> deployedAbilities;
    public static TinyBot PlayerControlledBot;
    private void Awake()
    {
        ClickableAbility.Activated = null;
        turnEnd.onClick.AddListener(EndPlayerTurn);
        PrimaryCursor.PlayerSelectedBot.AddListener(ShowControlForUnit);
        gameObject.SetActive(false);
    }

    readonly static KeyCode[] keyCodes = {KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9 };

    public void ShowControlForUnit(TinyBot bot)
    {
        gameObject.SetActive(true);
        VisualizeAbilityList(bot);
        unitPortrait.sprite = bot.Portrait;
        healthOverlay.UpdateHealth(bot);
    }

    void EndPlayerTurn()
    {
        if (PrimaryCursor.actionInProgress) return;
        gameObject.SetActive(false);
        TurnManager.EndTurn(PlayerControlledBot);
        ClickableAbility.EndUsableAbilityState();
    }

    void VisualizeAbilityList(TinyBot bot)
    {
        
        ClickableAbility.EndUsableAbilityState();
        deployedAbilities = new();
        bot.ActiveAbilities.PassDataToUI(clickableAbilities, AddActive);
        bot.PassiveAbilities.PassDataToUI(unclickableAbilities, AddPassive);

        void AddActive(ActiveAbility ability, ClickableAbility clickable)
        {
            clickable.Become(ability);
            deployedAbilities.Add(clickable);
        }

        void AddPassive(PassiveAbility ability, UnclickableAbility unclickable)
        {
            unclickable.Become(ability);
        }
    }

    private void Update()
    {
        if (deployedAbilities == null || !Input.anyKeyDown) return;
        for (int i = 0;i < deployedAbilities.Count; i++)
        {
            if (!Input.GetKeyDown(keyCodes[i])) continue;
            deployedAbilities[i].Activate();
        }
    }
}

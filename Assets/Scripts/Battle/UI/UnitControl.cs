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
        PrimaryCursor.PlayerSelectedBot.AddListener(PlayerControlBot);
        gameObject.SetActive(false);
    }

    readonly static KeyCode[] keyCodes = {KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9 };

    public void PlayerControlBot(TinyBot bot)
    {
        ReleaseBot();
        PlayerControlledBot = bot;
        gameObject.SetActive(true);
        VisualizeAbilityList();
        unitPortrait.sprite = bot.Portrait;
        UpdateOverlay();
        bot.AbilitiesChanged.AddListener(VisualizeAbilityList);
        bot.EndedTurn.AddListener(EndControl);
        bot.Stats.StatModified.AddListener(UpdateOverlay);
    }

    void UpdateOverlay()
    {
        healthOverlay.UpdateHealth(PlayerControlledBot);
    }

    void ReleaseBot()
    {
        if (PlayerControlledBot == null) return;
        PlayerControlledBot.Stats.StatModified.AddListener(UpdateOverlay);
        PlayerControlledBot.AbilitiesChanged.RemoveListener(VisualizeAbilityList);
        PlayerControlledBot.EndedTurn.RemoveListener(EndControl);
    }

    void EndPlayerTurn()
    {
        if (PrimaryCursor.ActionInProgress) return;
        TurnManager.EndTurn(PlayerControlledBot);
    }

    void EndControl()
    {
        gameObject.SetActive(false);
        ClickableAbility.EndUsableAbilityState();
    }

    void VisualizeAbilityList()
    {
        
        ClickableAbility.EndUsableAbilityState();
        deployedAbilities = new();
        PlayerControlledBot.ActiveAbilities.PassDataToUI(clickableAbilities, AddActive);
        PlayerControlledBot.PassiveAbilities.PassDataToUI(unclickableAbilities, AddPassive);

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
        if(Input.GetKeyDown(KeyCode.Space)) EndPlayerTurn();
        for (int i = 0;i < deployedAbilities.Count; i++)
        {
            if (!Input.GetKeyDown(keyCodes[i])) continue;
            deployedAbilities[i].Activate();
        }
    }
}

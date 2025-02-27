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
    public static TinyBot PlayerControlledBot { get; private set; }
    private void Awake()
    {
        turnEnd.onClick.AddListener(EndPlayerTurn);
        PrimaryCursor.PlayerSelectedBot.AddListener(PlayerControlBot);
        TinyBot.ClearActiveBot.AddListener(ReleaseBot);
    }

    //avoid race conditions with child UIs; their listeners should be initialized on awake
    private void Start()
    {
        gameObject.SetActive(false);
    }

    readonly static KeyCode[] keyCodes = {KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9 };

    void PlayerControlBot(TinyBot bot)
    {
        PlayerControlledBot = bot;
        gameObject.SetActive(true);
        VisualizeAbilityList();
        unitPortrait.sprite = bot.Portrait;
        UpdateOverlay();
        bot.AbilitiesChanged.AddListener(VisualizeAbilityList);
        bot.Stats.StatModified.AddListener(UpdateOverlay);
    }

    void UpdateOverlay()
    {
        healthOverlay.UpdateHealth(PlayerControlledBot);
    }

    void ReleaseBot()
    {
        gameObject.SetActive(false);
        BotCaster.ClearCasting.Invoke();
        if (PlayerControlledBot == null) return;
        PlayerControlledBot.Stats.StatModified.RemoveListener(UpdateOverlay);
        PlayerControlledBot.AbilitiesChanged.RemoveListener(VisualizeAbilityList);
        PlayerControlledBot = null;
        deployedAbilities = null;
    }

    void EndPlayerTurn()
    {
        if (PrimaryCursor.LockoutPlayer) return;
        TurnManager.EndTurn(PlayerControlledBot);
    }

    void VisualizeAbilityList()
    {
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
        if (Input.GetKeyDown(KeyCode.Space)) EndPlayerTurn();
        if (deployedAbilities == null || !Input.anyKeyDown) return;
        for (int i = 0;i < deployedAbilities.Count; i++)
        {
            if (!Input.GetKeyDown(keyCodes[i])) continue;
            deployedAbilities[i].Activate();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UnitControl : MonoBehaviour
{
    [SerializeField] ClickableAbility[] clickableAbilities;
    [SerializeField] Image unitPortrait;
    [SerializeField] Button turnEnd;

    [SerializeField] TurnManager turnManager;

    List<ClickableAbility> deployedAbilities;
    public static Ability ActiveSkill;
    static TinyBot ActiveBot;
    private void Awake()
    {
        ActiveSkill = null;
        turnEnd.onClick.AddListener(EndActiveTurn);
    }

    readonly static KeyCode[] keyCodes = {KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9 };

    public void ShowControlForUnit(TinyBot bot)
    {
        gameObject.SetActive(true);
        VisualizeAbilityList(bot);
        unitPortrait.sprite = bot.portrait;
        ActiveBot = bot;
    }

    void EndActiveTurn()
    {
        gameObject.SetActive(false);
        turnManager.EndTurn(ActiveBot);
        ActiveSkill = null;
        ActiveBot = null;
    }

    void VisualizeAbilityList(TinyBot bot)
    {
        ActiveSkill = null;
        ClickableAbility.clearActive?.Invoke();
        deployedAbilities = new();
        List<Ability> abilityList = bot.Abilities.Where(a => a != null).ToList();
        for(int i = 0; i < clickableAbilities.Count(); i++)
        {
            if(i < abilityList.Count)
            {
                Ability ability = abilityList[i];
                clickableAbilities[i].Become(ability, keyCodes[i]);
                deployedAbilities.Add(clickableAbilities[i]);
            }
            else
            {
                clickableAbilities[i].Clear();
            }
        }
    }

    private void Update()
    {
        if (deployedAbilities == null || !Input.anyKeyDown) return;
        for (int i = 0;i < deployedAbilities.Count; i++)
        {
            if (Input.GetKeyDown(keyCodes[i]))
            {
                deployedAbilities[i].Activate();
            }
        }
    }
}

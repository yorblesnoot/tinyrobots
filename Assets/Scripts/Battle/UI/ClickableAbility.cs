using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ClickableAbility : AbilityDisplay
{
    public static UnityEvent RefreshUsability = new();

    [SerializeField] Button button;
    [SerializeField] TMP_Text cooldown;
    [SerializeField] Image cooldownPanel;
    [SerializeField] List<Image> actionPoints;
    [SerializeField] Color manaColor;
    Color actionColor;

    [HideInInspector] public ActiveAbility Ability;
    BotCaster caster;
    private void Awake()
    {
        actionColor = actionPoints[0].color;
        RefreshUsability.AddListener(UpdateUsability);
        BotCaster.ClearCasting.AddListener(EndUsableAbilityState);
        button.onClick.AddListener(Activate);
    }

    private void OnDestroy()
    {
        RefreshUsability.RemoveListener(UpdateUsability);
        BotCaster.ClearCasting.RemoveListener(EndUsableAbilityState);
        button.onClick.RemoveListener(Activate);
    }

    public override void Become(Ability ability)
    {
        base.Become(ability);
        Ability = ability as ActiveAbility;
        caster = ability.Owner.Caster;
        button.interactable = true;
        ability.Owner.BeganTurn.AddListener(UpdateUsability);
        
        //update this with a different shape
        actionPoints.Select(pip => pip.color = Ability.CastingResource == StatType.MANA ? manaColor : actionColor);
        SetPips(ability.cost);
        UpdateUsability();
    }

    void UpdateUsability()
    {
        if (Ability == null) return;
        bool usable = Ability.IsAffordable() && Ability.IsAvailable();
        cooldownPanel.gameObject.SetActive(!usable);
        if (Ability.Locked) cooldown.text = "X";
        else if (Ability.CurrentCooldown > 0) cooldown.text = Ability.CurrentCooldown.ToString();
        else cooldown.text = "";
    }

    void OnDisable()
    {
        if (Ability != null) Ability.Owner.BeganTurn.RemoveListener(UpdateUsability);
        image.color = Color.white;
        Ability = null;
        SetPips(0);
    }

    void EndUsableAbilityState()
    {
        image.color = Color.white;
    }

    void SetPips(int pips)
    {
        for(int i = 0; i < actionPoints.Count; i++)
        {
            actionPoints[i].gameObject.SetActive(i < pips);
        }
    }

    public void Activate()
    {
        if (PrimaryCursor.LockoutPlayer) return;
        BotCaster.ClearCasting.Invoke();
        if (!caster.TryPrepare(Ability)) return;
        image.color = Color.red;
    }
}

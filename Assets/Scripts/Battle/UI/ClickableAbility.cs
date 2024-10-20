using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ClickableAbility : AbilityDisplay
{
    public static UnityEvent PlayerUsedAbility = new();

    [SerializeField] Button button;
    [SerializeField] TMP_Text cooldown;
    [SerializeField] Image cooldownPanel;
    [SerializeField] Transform pipHolder;
    [SerializeField] List<Image> actionPoints;

    [HideInInspector] public ActiveAbility Ability;
    public static ClickableAbility Activated;
    private void OnEnable()
    {
        PlayerUsedAbility.AddListener(UpdateUsability);
        TinyBot.ClearActiveBot.AddListener(CancelAbility);
    }

    

    public override void Become(Ability ability)
    {
        base.Become(ability);
        Ability = ability as ActiveAbility;
        button.interactable = true;
        ability.Owner.BeganTurn.AddListener(UpdateUsability);
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(Activate);
        SetPips(ability.cost);
        UpdateUsability();
    }

    void UpdateUsability()
    {
        if (Ability == null) return;
        bool usable = true;
        bool offCooldown = Ability.currentCooldown == 0;
        usable &= Ability.cost <= UnitControl.PlayerControlledBot.Stats.Current[StatType.ACTION];
        usable &= Ability.IsAvailable();
        usable &= offCooldown;
        cooldownPanel.gameObject.SetActive(!usable);
        cooldown.text = offCooldown ? "" : Ability.currentCooldown.ToString();
    }

    void OnDisable()
    {
        image.color = Color.white;
        SetPips(0);
        PlayerUsedAbility.RemoveListener(UpdateUsability);
        TinyBot.ClearActiveBot.RemoveListener(CancelAbility);
        button.onClick.RemoveAllListeners();
        Ability = null;
    }

    public static void EndUsableAbilityState()
    {
        if (Activated == null) return;
        Activated.image.color = Color.white;
        Activated.Ability.ReleaseLockOn();
        Activated = null;
    }

    public static void CancelAbility()
    {
        if(Activated == null) return;
        Activated.Ability.EndAbility();
        EndUsableAbilityState();
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
        if (!Ability.IsAvailable()) return;
        CancelAbility();
        Activated = this;
        Ability.LockOnTo(PrimaryCursor.Transform.gameObject, true);
        image.color = Color.red;
    }
}

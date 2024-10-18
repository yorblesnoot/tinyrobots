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
        PlayerUsedAbility.AddListener(OvercostOverlay);
        TinyBot.ClearActiveBot.AddListener(CancelAbility);
    }

    

    public override void Become(Ability ability)
    {
        base.Become(ability);
        Ability = ability as ActiveAbility;
        button.interactable = true;
        ability.Owner.BeganTurn.AddListener(UpdateCooldowns);
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(Activate);
        SetPips(ability.cost);
        UpdateCooldowns();
    }

    void OvercostOverlay()
    {
        if (Ability == null) return;
        bool unusuable = Ability.cost > UnitControl.PlayerControlledBot.Stats.Current[StatType.ACTION]
            || !Ability.IsAvailable();
        cooldownPanel.gameObject.SetActive(unusuable);
        cooldown.text = "";
    }

    void OnDisable()
    {
        image.color = Color.white;
        SetPips(0);
        PlayerUsedAbility.RemoveListener(OvercostOverlay);
        TinyBot.ClearActiveBot.RemoveListener(CancelAbility);
        button.onClick.RemoveAllListeners();
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

    

    public void UpdateCooldowns()
    {
        if (Ability == null) return;
        cooldownPanel.gameObject.SetActive(Ability.currentCooldown > 0);
        cooldown.text = Ability.currentCooldown.ToString();
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

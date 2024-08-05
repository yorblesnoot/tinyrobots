using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ClickableAbility : AbilityDisplay
{
    public static UnityEvent playerUsedAbility = new();

    [SerializeField] Button button;
    [SerializeField] TMP_Text cooldown;
    [SerializeField] Image cooldownPanel;
    [SerializeField] Transform pipHolder;
    [SerializeField] List<Image> actionPoints;

    [SerializeField] float dislacementModifier;
    float pointWidth;

    [HideInInspector] public ActiveAbility Ability;
    public static ClickableAbility Activated;
    private void Awake()
    {
        pointWidth = actionPoints[0].GetComponent<RectTransform>().rect.width;
        pointWidth *= dislacementModifier;
        playerUsedAbility.AddListener(OvercostOverlay);
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
        Ability = null;
        button.onClick.RemoveAllListeners();
    }

    public static void DeactivateSelectedAbility()
    {
        if (Activated == null) return;
        Activated.image.color = Color.white;
        Activated.Ability.ReleaseLockOn();
        Activated = null;
    }

    public static void Cancel()
    {
        if(Activated == null) return;
        Activated.Ability.EndAbility();
        DeactivateSelectedAbility();
    }

    

    public void UpdateCooldowns()
    {
        if (Ability == null) return;
        //Debug.Log(cooldownPanel + " panel " + Skill + " skill");
        cooldownPanel.gameObject.SetActive(Ability.currentCooldown > 0);
        cooldown.text = Ability.currentCooldown.ToString();
    }

    void SetPips(int pips)
    {
        for(int i = 0; i < actionPoints.Count; i++)
        {
            actionPoints[i].gameObject.SetActive(i < pips);
        }
        /*
        float newX = -pips * pointWidth / 2;
        Vector3 pos = pipHolder.transform.localPosition;
        pos.x = newX;
        pipHolder.transform.localPosition = pos;
        */
    }

    public void Activate()
    {
        if (!Ability.IsAvailable()) return;
        DeactivateSelectedAbility();
        Activated = this;
        Ability.LockOnTo(PrimaryCursor.Transform.gameObject, true);
        image.color = Color.red;
    }
}

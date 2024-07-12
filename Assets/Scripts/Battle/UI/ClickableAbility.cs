using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ClickableAbility : MonoBehaviour
{
    public static UnityEvent playerUsedAbility = new();

    [SerializeField] protected Image image;
    [SerializeField] Button button;
    [SerializeField] TMP_Text cooldown;
    [SerializeField] Image cooldownPanel;
    [SerializeField] Transform pipHolder;
    [SerializeField] List<Image> actionPoints;

    [SerializeField] float dislacementModifier;
    float pointWidth;

    public ActiveAbility Skill;
    public static ClickableAbility Active;
    private void Awake()
    {
        pointWidth = actionPoints[0].GetComponent<RectTransform>().rect.width;
        pointWidth *= dislacementModifier;
        playerUsedAbility.AddListener(OvercostOverlay);
    }

    void OvercostOverlay()
    {
        if (Skill == null) return;
        bool unusuable = Skill.cost > UnitControl.PlayerControlledBot.Stats.Current[StatType.ACTION]
            || !Skill.IsAvailable();
        cooldownPanel.gameObject.SetActive(unusuable);
        cooldown.text = "";
    }

    public void Hide()
    {
        button.enabled = false;
        Active.image.color = Color.white;
        SetPips(0);
        Clear();
    }

    public static void DeactivateSelectedAbility()
    {
        if (Active == null) return;
        Active.image.color = Color.white;
        PrimaryCursor.ToggleAirCursor(UnitControl.PlayerControlledBot == null || UnitControl.PlayerControlledBot.PrimaryMovement.Style == MoveStyle.FLY);
        Active.Skill.ReleaseLockOn();
        Active = null;
    }

    public static void Cancel()
    {
        if(Active == null) return;
        Active.Skill.EndAbility();
        DeactivateSelectedAbility();
    }

    public void Become(ActiveAbility ability)
    {
        ability.Owner.BeganTurn.AddListener(UpdateCooldowns);
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(Activate);
        SetPips(ability.cost);
        UpdateCooldowns();
    }

    public void UpdateCooldowns()
    {
        if (Skill == null) return;
        //Debug.Log(cooldownPanel + " panel " + Skill + " skill");
        cooldownPanel.gameObject.SetActive(Skill.currentCooldown > 0);
        cooldown.text = Skill.currentCooldown.ToString();
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

    public void Clear()
    {
        Skill = null;
        button.onClick.RemoveAllListeners();
        
    }

    public void Activate()
    {
        if (!Skill.IsAvailable()) return;
        DeactivateSelectedAbility();
        Active = this;
        PrimaryCursor.ToggleAirCursor(true);
        Skill.LockOnTo(PrimaryCursor.Transform.gameObject, true);
        image.color = Color.red;
    }
}

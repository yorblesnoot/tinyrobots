using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClickableAbility : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] Button button;
    [SerializeField] TMP_Text cooldown;
    [SerializeField] Image cooldownPanel;
    [HideInInspector] public Ability Skill;

    
    [SerializeField] Transform pipHolder;
    [SerializeField] List<Image> actionPoints;

    [SerializeField] float dislacementModifier;
    float pointWidth;

    public static ClickableAbility Active;
    private void Awake()
    {
        pointWidth = actionPoints[0].GetComponent<RectTransform>().rect.width;
        pointWidth *= dislacementModifier;
    }

    public static void DeactivateSelectedAbility()
    {
        if (Active == null) return;
        Active.image.color = Color.white;
        PrimaryCursor.SetCursorMode(UnitControl.ActiveBot == null ? CursorType.GROUND : UnitControl.ActiveBot.PrimaryMovement.PreferredCursor);
        Active.Skill.ReleaseLockOn();
        Active = null;
    }

    public static void CancelAbility()
    {
        if(Active == null) return;
        Active.Skill.NeutralAim();
        DeactivateSelectedAbility();
    }

    public void Become(Ability ability, KeyCode key)
    {
        ability.Owner.beganTurn.AddListener(UpdateCooldowns);
        gameObject.SetActive(true);
        Skill = ability;
        image.sprite = ability.icon;
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
        gameObject.SetActive(false);
    }

    public void Activate()
    {
        if (Skill.currentCooldown > 0) return;
        DeactivateSelectedAbility();
        Active = this;
        PrimaryCursor.SetCursorMode(Skill.PreferredCursor);
        Skill.LockOnTo(PrimaryCursor.Transform.gameObject, true);
        image.color = Color.red;
    }
}

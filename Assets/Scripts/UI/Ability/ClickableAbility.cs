using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClickableAbility : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] Button button;
    [SerializeField] TMP_Text letter;
    public Ability Skill;

    
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

    public static void Deactivate()
    {
        if (Active == null) return;
        
        Active.image.color = Color.white;
        PrimaryCursor.SetCursorMode(UnitControl.ActiveBot == null ? CursorType.GROUND : UnitControl.ActiveBot.PrimaryMovement.PreferredCursor);
        Active.Skill.ToggleSkillTargeting(false);
        Active = null;
    }

    public void Become(Ability ability, KeyCode key)
    {
        gameObject.SetActive(true);
        Skill = ability;
        image.sprite = ability.icon;
        letter.text = key.ToString();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(Activate);
        SetPips(ability.cost);
    }

    void SetPips(int pips)
    {
        for(int i = 0; i < actionPoints.Count; i++)
        {
            actionPoints[i].gameObject.SetActive(i < pips);
        }
        float newX = -pips * pointWidth / 2;
        Vector3 pos = pipHolder.transform.localPosition;
        pos.x = newX;
        pipHolder.transform.localPosition = pos;
    }

    public void Clear()
    {
        Skill = null;
        button.onClick.RemoveAllListeners();
        gameObject.SetActive(false);
    }

    public void Activate()
    {
        Deactivate();
        Active = this;
        PrimaryCursor.SetCursorMode(Skill.PreferredCursor);
        Skill.ToggleSkillTargeting(true);
        image.color = Color.red;
    }
}

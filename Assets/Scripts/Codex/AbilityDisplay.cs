using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AbilityDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector] public Ability Skill;
    [SerializeField] protected Image image;

    public virtual void Become(Ability ability)
    {
        gameObject.SetActive(true);
        Skill = ability;
        image.sprite = ability.icon;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        AbilityTooltip.Show(Skill, transform.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        AbilityTooltip.Hide();
    }
}

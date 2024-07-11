using UnityEngine;
using UnityEngine.UI;

public class AbilityDisplay : MonoBehaviour
{
    [HideInInspector] public ActiveAbility Skill;
    [SerializeField] protected Image image;
    public virtual void Become(ActiveAbility ability)
    {
        gameObject.SetActive(true);
        Skill = ability;
        image.sprite = ability.icon;
    }
}

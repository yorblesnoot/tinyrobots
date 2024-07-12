using UnityEngine;
using UnityEngine.UI;

public class AbilityDisplay : MonoBehaviour
{
    [HideInInspector] public Ability Skill;
    [SerializeField] protected Image image;

    public virtual void Become(Ability ability)
    {
        gameObject.SetActive(true);
        Skill = ability;
        image.sprite = ability.icon;
    }
}

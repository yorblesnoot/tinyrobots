using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClickableAbility : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] Button button;

    public static Ability Active { get; private set; }
    public void Become(Ability ability)
    {
        gameObject.SetActive(true);
        Debug.Log(image);
        Debug.Log(ability);
        image.sprite = ability.icon;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => Activate(ability));
    }

    void Activate(Ability ability)
    {
        Active = ability;
    }
}

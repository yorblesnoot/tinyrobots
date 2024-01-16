using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClickableAbility : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] Button button;
    [SerializeField] TMP_Text letter;
    Ability thisAbility;

    public static Ability Active { get; private set; }
    public void Become(Ability ability, KeyCode key)
    {
        gameObject.SetActive(true);
        thisAbility = ability;
        image.sprite = ability.icon;
        letter.text = key.ToString();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(Activate);
    }

    public void Activate()
    {
        Active = thisAbility;
        image.color = Color.red;
    }
}

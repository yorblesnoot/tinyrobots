using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityTooltip : MonoBehaviour
{
    static AbilityTooltip instance;
    [SerializeField] float heightModifier = 50;
    [SerializeField] bool usePosition;

    [Header("Components")]
    [SerializeField] TMP_Text abilityName;
    [SerializeField] TMP_Text abilityType;
    [SerializeField] TMP_Text damage;
    [SerializeField] TMP_Text range;
    [SerializeField] TMP_Text aoe;
    [SerializeField] TMP_Text cooldown;
    [SerializeField] TMP_Text cost;
    [SerializeField] TMP_Text description;

    Image image;
    static float showHeight;
    private void Awake()
    {
        instance = this;
        gameObject.SetActive(false);
        image = GetComponent<Image>();
        showHeight = image.rectTransform.rect.height/2 + heightModifier;
    }

    public static void Show(Ability ability, Vector3 position)
    {
        instance.gameObject.SetActive(true);
        instance.abilityName.text = ability.gameObject.name;
        instance.abilityType.text = ability.IsActive() ? "Active Ability" : "Passive Ability";
        instance.damage.text = ability.EffectMagnitude + ability.EffectDescription;
        instance.range.text = ability.range + " Ft";
        instance.cooldown.text = ability.cooldown + " Turns";
        instance.cost.text = ability.cost + " AP";
        instance.description.text = ability.Description;

        if (instance.usePosition) return;
        position = instance.transform.parent.InverseTransformPoint(position);
        position.y += position.y > 0 ? -showHeight : showHeight;
        instance.transform.localPosition = position;
    }

    public static void Hide()
    {
        instance.gameObject.SetActive(false);
    }
}

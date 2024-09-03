using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AbilityOverviewPanel : MonoBehaviour
{
    static AbilityOverviewPanel instance;

    [SerializeField] TMP_Text abilityName;
    [SerializeField] TMP_Text abilityType;
    [SerializeField] TMP_Text damage;
    [SerializeField] TMP_Text range;
    [SerializeField] TMP_Text aoe;
    [SerializeField] TMP_Text cooldown;
    [SerializeField] TMP_Text cost;
    [SerializeField] TMP_Text description;
    private void Awake()
    {
        instance = this;
        gameObject.SetActive(false);
    }

    public static void Show(Ability ability, Vector3 position)
    {
        instance.gameObject.SetActive(true);
        instance.transform.position = position;
        instance.abilityName.text = ability.gameObject.name;
        instance.abilityType.text = ability.IsActive() ? "Active Ability" : "Passive Ability";
        instance.damage.text = ability.damage + " Damage";
        instance.range.text = ability.range + " Ft";
        instance.cooldown.text = ability.cooldown + " Turns";
        instance.cost.text = ability.cost + " AP";
        instance.description.text = ability.Description;
    }

    public static void Hide()
    {
        instance.gameObject.SetActive(false);
    }
}

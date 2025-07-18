using TMPro;
using UnityEngine;

public class AbilityTooltip : TooltipBase
{
    static AbilityTooltip instance;
    

    [Header("Components")]
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
        instance.abilityName.text = ability.gameObject.name;
        instance.abilityType.text = ability.IsActive ? "Active Ability" : "Passive Ability";
        instance.damage.text = ability.GetEffectPhrases();
        instance.range.text = ability.range + " Ft";
        instance.cooldown.text = ability.cooldown + " Turns";
        instance.cost.text = ability.cost + $" {GetCostName(ability)}";
        instance.description.text = DynamicAbilityDescription.Describe(ability);

        instance.SetPosition(position);
    }

    static string GetCostName(Ability ability)
    {
        string name = " AP";
        ActiveAbility active = ability as ActiveAbility;
        if(active != null && active.costsMana) name = " MP";
        return name;
    }

    public static void Hide()
    {
        instance.gameObject.SetActive(false);
    }
}

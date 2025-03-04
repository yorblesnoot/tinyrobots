using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetaAbility : Ability
{
    public override bool IsActive => false;

    protected override AbilityEffect[] Effects => triggeredEffects;
    [SerializeField] AbilityEffect[] triggeredEffects;
    [SerializeField] bool onlyLastTriggers = false;
    [SerializeField] ModType abilityMod;
    [SerializeField] int modValue;

    List<AlternatePart> alternateParts;
    AlternatePart active;
    

    public override void Initialize(TinyBot botUnit)
    {
        base.Initialize(botUnit);
        foreach (AbilityEffect effect in Effects) effect.Initialize(this);
        alternateParts = new();
        
        foreach(var subTree in SubTrees)
        {
            AlternatePart alternate = CreateAlternate(botUnit, subTree);
            AttachTriggers(alternate);
            alternateParts.Add(alternate);
        }
    }

    public void SetActiveAlternate(int index = -1)
    {

        if(active != null)
        {
            ToggleAlternate(active, false);
        }
        List<AlternatePart> options = new(alternateParts);
        options.Remove(active);
        if (options.Count == 0) index = 0;
        else if (index < 0) index = Random.Range(0, options.Count);
        active = index < options.Count ? options[index] : null;
        ToggleAlternate(active, true);

    }

    void ToggleAlternate(AlternatePart alternate, bool on)
    {
        if (alternate == null) return;
        alternate.Root.SetActive(on);
        foreach (Ability ability in alternate.Abilities)
        {
            ability.ModifyOn(Owner, on);
        }
    }

    private AlternatePart CreateAlternate(TinyBot botUnit, TreeNode<ModdedPart> subTree)
    {
        PrimaryMovement locomotion = null;
        AlternatePart alternate = new();
        List<PartModifier> parts = new();
        GameObject spawned = BotAssembler.RecursiveConstruction(subTree, parts, botUnit.Stats, ref locomotion);
        foreach (var part in parts)
        {
            foreach(Ability ability in part.Abilities)
            {
                alternate.Abilities.Add(ability);
                ability.Initialize(botUnit);
                ModdedPart.ApplyMod(ability, new KeyValuePair<ModType, int>(abilityMod, modValue));
            }
        }
        alternate.Root = spawned;
        spawned.transform.SetParent(emissionPoint, false);
        spawned.transform.localPosition = Vector3.zero;
        spawned.SetActive(false);
        return alternate;
    }

    void AttachTriggers(AlternatePart alternate)
    {
        List<ActiveAbility> actives = new();
        foreach (Ability ability in alternate.Abilities)
        {
            ActiveAbility active = ability as ActiveAbility;
            if(active == null) continue;
            actives.Add(active);
        }
        if (actives.Count == 0) return;
        if (onlyLastTriggers) actives[^1].Used.AddListener(() => StartCoroutine(TriggerEffect()));
        else foreach (ActiveAbility ability in actives) ability.Used.AddListener(() => StartCoroutine(TriggerEffect()));
    }

    IEnumerator TriggerEffect()
    {
        List<Vector3> trajectory = new() { Owner.TargetPoint.position, Owner.TargetPoint.position };
        List<Targetable> targets = new() { Owner };
        foreach (AbilityEffect effect in triggeredEffects)
        {
            yield return effect.PerformEffect(Owner, trajectory, targets);
        }
    }

    protected override void AddTo(TinyBot bot)
    {
        bot.MetaAbilities.Add(this);
        ToggleAlternate(active, true);
    }

    protected override void RemoveFrom(TinyBot bot)
    {
        bot.MetaAbilities.Remove(this);
        ToggleAlternate(active, false);
    }
}

class AlternatePart
{
    public GameObject Root;
    public List<Ability> Abilities = new();
}

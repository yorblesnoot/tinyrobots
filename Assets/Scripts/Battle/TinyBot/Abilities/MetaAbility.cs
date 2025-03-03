using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetaAbility : Ability
{
    public override bool IsActive => false;

    protected override AbilityEffect[] Effects => throw new System.NotImplementedException();
    List<AlternatePart> alternateParts;
    AlternatePart active;
    

    public override void Initialize(TinyBot botUnit)
    {
        base.Initialize(botUnit);

        alternateParts = new();
        
        foreach(var subTree in SubTrees)
        {
            AlternatePart alternate = CreateAlternate(botUnit, subTree);
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
        if(index < 0) index = Random.Range(0, options.Count);
        active = options[index];
        ToggleAlternate(active, true);

    }

    void ToggleAlternate(AlternatePart alternate, bool on)
    {
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
            alternate.Abilities.AddRange(part.Abilities);
        }
        alternate.Root = spawned;
        spawned.transform.SetParent(emissionPoint, false);
        spawned.SetActive(false);
        return alternate;
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

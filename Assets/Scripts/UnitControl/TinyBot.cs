using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Allegiance
{
    PLAYER,
    ALLIED,
    ENEMY
}
public class TinyBot : MonoBehaviour
{
    public TreeNode<GameObject> componentParts;
    List<GameObject> flatParts;

    [SerializeField] GameObject selectBrackets;
    [SerializeField] BotStateFeedback feedback;
    public Transform headshotPosition;

    [HideInInspector] public Sprite portrait;
    [HideInInspector] public Allegiance allegiance;
    public bool availableForTurn;

    public BotStats Stats = new();

    public List<Ability> Abilities { get; private set; }
    public void Initialize(TreeNode<GameObject> tree)
    {
        componentParts = tree;
        flatParts = componentParts.Flatten().ToList();
        GenerateAbilityList();
    }

    public bool SpendAbilityPoints(Ability ability)
    {
        if (ability.cost > Stats.Current[StatType.ACTION]) return false;
        Stats.Current[StatType.ACTION] -= ability.cost;
        return true;
    }

    public void BeginTurn()
    {
        Stats.SetToMax(StatType.ACTION);
        Stats.SetToMax(StatType.MOVEMENT);
    }

    void GenerateAbilityList()
    {
        Abilities = new();
        foreach (var componentPart in flatParts)
        {
            Abilities.Add(componentPart.GetComponent<Ability>());
        }
    }

    public void BecomeActiveUnit(bool active)
    {
        selectBrackets.SetActive(active);
        if (active) gameObject.layer = 6;
        else gameObject.layer = 0;
    }
    

    void Die() { }

    public void ReceiveDamage(int damage)
    {
        feedback.QueuePopup(damage, Color.red);
        Stats.Current[StatType.HEALTH] = Math.Clamp(Stats.Current[StatType.HEALTH] - damage, 0, Stats.Max[StatType.HEALTH]);
        if(Stats.Current[StatType.HEALTH] == 0) Die();
    }

    private void OnMouseEnter()
    {
        PrimaryCursor.ToggleUnitSnap(this);
    }

    private void OnMouseExit()
    {
        PrimaryCursor.ToggleUnitSnap();
    }
}

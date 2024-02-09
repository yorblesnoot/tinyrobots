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

    [SerializeField] GameObject selectBrackets;
    [SerializeField] BotStateFeedback feedback;
    public Transform headshotPosition;

    [HideInInspector] public Sprite portrait;
    [HideInInspector] public Allegiance allegiance;
    public bool availableForTurn;

    public PrimaryMovement PrimaryMovement;

    public BotStats Stats = new();

    public List<Ability> Abilities { get; private set; }
    public void Initialize(List<Ability> abilities, PrimaryMovement primaryMovement)
    {
        Abilities = abilities;
        PrimaryMovement = primaryMovement;
    }

    public bool AttemptToSpendResource(float resource, StatType statType)
    {
        if (resource > Stats.Current[statType]) return false;
        Stats.Current[statType] -= resource;
        return true;
    }

    public void BeginTurn()
    {
        Stats.SetToMax(StatType.ACTION);
        Stats.SetToMax(StatType.MOVEMENT);
    }

    public void BecomeActiveUnit()
    {
        UnitControl.ActiveBot = this;
        gameObject.layer = 6;
    }

    public void ClearActiveUnit()
    {
        UnitControl.ActiveBot = null;
        selectBrackets.SetActive(false);
        gameObject.layer = 0;
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

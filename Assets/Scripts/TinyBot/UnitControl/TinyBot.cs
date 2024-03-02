using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public enum Allegiance
{
    PLAYER,
    ALLIED,
    ENEMY
}
public class TinyBot : MonoBehaviour
{
    [SerializeField] float deathExplodeMaxForce;
    [SerializeField] GameObject selectBrackets;
    [SerializeField] BotStateFeedback feedback;
    
    public Transform headshotPosition;

    [HideInInspector] public Sprite portrait;
    [HideInInspector] public Allegiance allegiance;
    [HideInInspector] public bool availableForTurn;

    [HideInInspector] public PrimaryMovement PrimaryMovement;

    public BotStats Stats = new();

    public Transform ChassisPoint;

    public static UnityEvent ClearActiveBot = new();
    public UnityEvent beganTurn = new();

    BotAI botAI;

    public List<Ability> Abilities { get; private set; }
    List<GameObject> Parts;
    public void Initialize(List<Ability> abilities, List<GameObject> parts, PrimaryMovement primaryMovement)
    {
        Parts = parts;
        Abilities = abilities;
        PrimaryMovement = primaryMovement;
        PrimaryMovement.Owner = this;
        ClearActiveBot.AddListener(ClearActiveUnit);
    }

    public bool AttemptToSpendResource(int resource, StatType statType)
    {
        if (resource > Stats.Current[statType]) return false;
        Stats.Current[statType] -= resource;
        return true;
    }

    public void BeginTurn()
    {
        beganTurn?.Invoke();
        Stats.SetToMax(StatType.ACTION);
        Stats.SetToMax(StatType.MOVEMENT);
        if (allegiance == Allegiance.PLAYER) availableForTurn = true;
        else
        {
            botAI ??= new(this);
            MainCameraControl.CutToUnit(this);
            StartCoroutine(botAI.TakeTurn());
        }
    }

    public void BecomeActiveUnit()
    {
        selectBrackets.SetActive(true);
        UnitControl.ActiveBot = this;
        ToggleActiveLayer(true);
    }

    public void ToggleActiveLayer(bool active)
    {
        if(active) gameObject.layer = 6;
        else gameObject.layer = 0;
    }

    public void ClearActiveUnit()
    {
        UnitControl.ActiveBot = null;
        selectBrackets.SetActive(false);
        ToggleActiveLayer(false);
    }

    readonly float minForce = .1f;
    void Die()
    {
        foreach(var part in Parts)
        {
            Rigidbody rigidPart = part.AddComponent<Rigidbody>();
            Vector3 explodeForce = new(Random.Range(minForce, deathExplodeMaxForce), 
                Random.Range(minForce, deathExplodeMaxForce), 
                Random.Range(minForce, deathExplodeMaxForce));
            rigidPart.velocity = explodeForce;
        }
        TurnManager.RemoveTurnTaker(this);
        Destroy(gameObject, 5f);
    }

    public void ReceiveDamage(int damage)
    {
        feedback.QueuePopup(damage, Color.red);
        Stats.Current[StatType.HEALTH] = Math.Clamp(Stats.Current[StatType.HEALTH] - damage, 0, Stats.Max[StatType.HEALTH]);
        TurnManager.UpdateHealth(this);
        if(Stats.Current[StatType.HEALTH] == 0) Die();
    }

    private void OnMouseEnter()
    {
        if(UnitControl.ActiveBot != this) PrimaryCursor.SnapToUnit(this);
    }

    private void OnMouseExit()
    {
        if(PrimaryCursor.TargetedBot == this) PrimaryCursor.Unsnap();
    }
}

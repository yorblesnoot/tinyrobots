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

    int maxHealth;
    int currentHealth;

    public List<Ability> Abilities { get; private set; }
    public void Initialize(TreeNode<GameObject> tree)
    {
        componentParts = tree;
        flatParts = componentParts.Flatten().ToList();
        GenerateAbilityList();
    }

    void GenerateAbilityList()
    {
        Abilities = new();
        foreach (var componentPart in flatParts)
        {
            Abilities.Add(componentPart.GetComponent<Ability>());
        }
    }

    private void Update()
    {
        if (PrimaryCursor.SelectedBot != this) return;
        //TurnTowardsCursor();
    }

    private void TurnTowardsCursor()
    {
        Vector3 direction = (PrimaryCursor.Transform.position - transform.position).normalized;
        Quaternion lookRot = Quaternion.LookRotation(direction, Vector3.up);
        Vector3 eulerLook = lookRot.eulerAngles;
        eulerLook.x = 0;
        eulerLook.z = 0;
        transform.rotation = Quaternion.Euler(eulerLook);
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
        currentHealth = Math.Clamp(currentHealth - damage, 0, maxHealth);
        if(currentHealth == 0) Die();
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

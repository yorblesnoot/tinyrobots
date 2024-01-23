using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TinyBot : MonoBehaviour
{
    public TreeNode<GameObject> componentParts;
    List<GameObject> flatParts;

    public GameObject selectBrackets;
    Collider collider;

    int maxHealth;
    int currentHealth;

    public void Initialize(TreeNode<GameObject> tree)
    {
        componentParts = tree;
        flatParts = componentParts.Flatten().ToList();
        collider = GetComponent<Collider>();
    }

    internal List<Ability> GenerateAbilityList()
    {
        List<Ability> abilities = new();
        foreach (var componentPart in flatParts)
        {
            abilities.Add(componentPart.GetComponent<Ability>());
        }
        return abilities;
    }

    private void Update()
    {
        if (PrimaryCursor.SelectedBot != this) return;
        TurnTowardsCursor();
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
        Debug.Log("damaged");
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

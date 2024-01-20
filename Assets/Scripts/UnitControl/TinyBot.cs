using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TinyBot : MonoBehaviour
{
    public TreeNode<GameObject> componentParts;
    List<GameObject> flatParts;

    public void Initialize(TreeNode<GameObject> tree)
    {
        componentParts = tree;
        flatParts = componentParts.Flatten().ToList();
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

    private void OnMouseEnter()
    {
        PrimaryCursor.ToggleUnitLock(this);
    }

    private void OnMouseExit()
    {
        PrimaryCursor.ToggleUnitLock();
    }
}

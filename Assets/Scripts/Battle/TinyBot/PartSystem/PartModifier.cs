using System.Collections.Generic;
using UnityEngine;

public class PartModifier : MonoBehaviour
{
    [SerializeField] Transform abilityContainer;

    public List<TreeNode<ModdedPart>> SubTrees;
    public List<Renderer> mainRenderers;
    [HideInInspector] public Ability[] Abilities;
    [HideInInspector] public ModdedPart SourcePart;

    private void Awake()
    {
        Abilities = abilityContainer.GetComponentsInChildren<Ability>();
    }
}

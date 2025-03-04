using System.Collections.Generic;
using UnityEngine;

public class PartModifier : MonoBehaviour
{
    [SerializeField] Transform abilityContainer;

    
    public List<Renderer> mainRenderers;
    [HideInInspector] public AttachmentPoint[] AttachmentPoints;
    [HideInInspector] public Ability[] Abilities;
    [HideInInspector] public ModdedPart SourcePart;

    private void Awake()
    {
        Abilities = new Ability[abilityContainer.childCount];
        for (int i = 0; i < Abilities.Length; i++)
        {
            Abilities[i] = abilityContainer.GetChild(i).GetComponent<Ability>();
        }
        AttachmentPoints = gameObject.GetComponentsInChildren<AttachmentPoint>();
    }

    public void AddSubTree(TreeNode<ModdedPart> subTree)
    {
        foreach(Ability ability in Abilities)
        {
            ability.SubTrees ??= new();
            ability.SubTrees.Add(subTree);
        }
    }
}

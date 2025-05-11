using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PartModifier : MonoBehaviour
{
    [SerializeField] bool orientUpward;
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

    public void AttachPart(Transform attachmentPoint)
    {
        if(attachmentPoint != null) transform.SetParent(attachmentPoint, false);
        if (orientUpward)
        {
            Vector3 slotUp = Vector3.ProjectOnPlane(Vector3.up, attachmentPoint.forward);
            transform.rotation = Quaternion.LookRotation(slotUp, attachmentPoint.forward);
        }
        else transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
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
